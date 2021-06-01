using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace gRPCNet.Client
{
    public class GrpcChannelService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<GrpcChannelService> _logger;
        private readonly IFileLogger _fileLogger;

        public GrpcChannelService(
            IConfiguration configuration, 
            ILogger<GrpcChannelService> logger, 
            IFileLogger fileLogger)
        {
            _configuration = configuration;
            _logger = logger;
            _fileLogger = fileLogger;

            var token = GetTokenAsync().Result;
            if (string.IsNullOrEmpty(token)) 
            {
                Channel = null;
                return;
            }

            var callCredentials = CallCredentials.FromInterceptor((context, metadata) =>
            {
                metadata.Add("Authorization", $"Bearer {token}");
                return Task.CompletedTask;
            });

            // to disable TLS
            //GrpcClientFactory.AllowUnencryptedHttp2 = true;
            Channel = GrpcChannel.ForAddress(
                _configuration.GetSection("gRPCServices:Address").Get<string>(),
                new GrpcChannelOptions
                {
                    //Configure client options. gRPC client configuration is set on GrpcChannelOptions. (https://docs.microsoft.com/en-us/aspnet/core/grpc/configuration?view=aspnetcore-3.1)
                    MaxReceiveMessageSize = 1 * 1024 * 1024, // 1 MB
                    MaxSendMessageSize = 1 * 1024 * 1024, // 1 MB
                    Credentials = _configuration.GetSection("gRPCServices:Authentication").Get<string>().Equals("Bearer", StringComparison.InvariantCultureIgnoreCase) ?
                        ChannelCredentials.Create(new SslCredentials(), callCredentials) :
                        null,
                    HttpHandler = _configuration.GetSection("gRPCServices:Authentication").Get<string>().Equals("ClientCertificate", StringComparison.InvariantCultureIgnoreCase) ?
                        CreateCertifiedHandler() : 
                        CreateHandler()
                });
        }

        public GrpcChannel Channel { get; }

        private async Task<string> GetTokenAsync() 
        {
            string token = string.Empty;
            using (var tokenHandler = new SocketsHttpHandler())
            {
                using (var tokenClient = new HttpClient(tokenHandler))
                {
                    var tokenEndpoint = $"{_configuration.GetSection("gRPCServices:Address").Get<string>()}/oauth/token";
                    var content = new StringContent(
                        content: $"grant_type=client_credentials&scope={_configuration.GetSection("gRPCServices:ApiScope").Get<string>()}",
                        encoding: Encoding.UTF8,
                        mediaType: "application/x-www-form-urlencoded");
                    var clientId = _configuration.GetSection("gRPCServices:ClientId").Get<string>();
                    var clientSecret = _configuration.GetSection("gRPCServices:ClientSecret").Get<string>();
                    var authorization = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

                    tokenClient.DefaultRequestVersion = new Version(2, 0);
                    tokenClient.DefaultRequestHeaders.Add("Authorization", $"Basic {authorization}");
                    try
                    {
                        using (var tokenResponse = await tokenClient.PostAsync(tokenEndpoint, content))
                        {
                            if (tokenResponse.IsSuccessStatusCode)
                            {
                                token = await tokenResponse.Content.ReadAsStringAsync();
                                _logger.LogInformation($"GrpcChannelService received jwt token: {token}");
                                _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} GrpcChannelService received jwt token: {token}");
                            }
                            else
                            {
                                _logger.LogWarning($"GrpcChannelService cannot receive jwt token: StatusCode {tokenResponse.StatusCode}");
                                _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} GrpcChannelService cannot receive jwt token: StatusCode {tokenResponse.StatusCode}");
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"GrpcChannelService->GetTokenAsync: {ex}");
                        _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} GrpcChannelService->GetTokenAsync: {ex}");
                    }
                }
            }
            return token;
        }

        private HttpClientHandler CreateHandler()
        {
            return new HttpClientHandler();
        }

        private HttpClientHandler CreateCertifiedHandler() 
        {
            var handler = new HttpClientHandler();
            
            X509Certificate2 cert = null;
            if (_configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertFileEnabled").Get<bool>()) 
            {
                cert = GetCertificateFromFile(
                            _configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertFile:Certificate:Path").Get<string>(),
                            _configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertFile:Certificate:Password").Get<string>());
            }
            else if (_configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertStoreEnabled").Get<bool>())
            {
                if (Enum.TryParse(
                    _configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertStore:Certificate:Location").Get<string>(),
                    true,
                    out StoreLocation storeLocation))
                {
                    cert = GetCertificateFromStore(
                        _configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertStore:Certificate:Store").Get<string>(),
                        storeLocation,
                        _configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertStore:Certificate:ThumbPrint").Get<string>());
                }
            }
            if (cert == null) throw new NotImplementedException("cert is null");

            //if (!cert.HasPrivateKey) throw new NotImplementedException("canot access private key");

            handler.ClientCertificates.Add(cert);
            
            return handler;
        }

        private X509Certificate2 GetCertificateFromStore(string storeName, StoreLocation storeLocation, string certThumbPrint)
        {
            // Get the certificate store.
            X509Store store = new X509Store(storeName, storeLocation);
            try
            {
                store.Open(OpenFlags.ReadOnly);
                // Place all certificates in an X509Certificate2Collection object.
                X509Certificate2Collection certCollection = store.Certificates;

                // If using a certificate with a trusted root you do not need to FindByTimeValid, instead: .Find(X509FindType.FindByThumbprint, certThumbPrint, true);
                X509Certificate2Collection signingCert = certCollection.Find(X509FindType.FindByThumbprint, certThumbPrint, true);

                //X509Certificate2Collection currentCerts = certCollection.Find(X509FindType.FindByTimeValid, DateTime.Now, false);
                //X509Certificate2Collection signingCert = currentCerts.Find(X509FindType.FindByThumbprint, certThumbPrint, false);

                if (signingCert.Count == 0)
                    return null;
                // Return the first certificate in the collection, has the right name and is current.
                return signingCert[0];
            }
            finally
            {
                store.Close();
            }
        }

        private X509Certificate2 GetCertificateFromFile(string path, string password)
        {
            return new X509Certificate2(path, password);
        }
    }
}
