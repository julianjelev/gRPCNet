using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace gRPCNet.ServerAPI
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            // Run with console or service
            var isService = !(Debugger.IsAttached || args.Contains("--console"));
            var pathToContentRoot = Directory.GetCurrentDirectory();

            var webHostArgs = args.Where(arg => arg != "--console").ToArray();

            if (isService)
            {
                var pathToExe = Process.GetCurrentProcess().MainModule.FileName;
                pathToContentRoot = Path.GetDirectoryName(pathToExe);
            }

            var host = WebHost.CreateDefaultBuilder(webHostArgs)
                .UseContentRoot(pathToContentRoot)
                .ConfigureAppConfiguration(config =>
                {
                    config.SetBasePath(pathToContentRoot)
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .AddJsonFile("certificate.json", optional: true, reloadOnChange: true);
                })
                .ConfigureKestrel((hostContext, options) =>
                {
                    IPAddress ip = null;

                    if (hostContext.Configuration.GetSection("ServerConfigurations:Host").Exists() && !string.IsNullOrWhiteSpace(hostContext.Configuration.GetSection("ServerConfigurations:Host").Value))
                    {
                        IPHostEntry ipHostEntry = Dns.GetHostEntry(hostContext.Configuration.GetSection("ServerConfigurations:Host").Get<string>());
                        foreach (var item in ipHostEntry.AddressList)
                            if (item.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                ip = IPAddress.Parse(item.ToString());
                                break;
                            }
                    }
                    if (ip == null)
                        ip = IPAddress.Parse(hostContext.Configuration.GetSection("ServerConfigurations:IP").Get<string>());

                    X509Certificate2 serv_cert = null;

                    if (hostContext.Configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertFileEnabled").Get<bool>())
                    {
                        serv_cert = GetCertificateFromFile(
                            hostContext.Configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertFile:Certificate:Path").Get<string>(),
                            hostContext.Configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertFile:Certificate:Password").Get<string>());
                    }
                    else if (hostContext.Configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertStoreEnabled").Get<bool>())
                    {
                        if (Enum.TryParse(
                            hostContext.Configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertStore:Certificate:Location").Get<string>(),
                            true,
                            out StoreLocation storeLocation))
                        {
                            serv_cert = GetCertificateFromStore(
                                hostContext.Configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertStore:Certificate:Store").Get<string>(),
                                storeLocation,
                                hostContext.Configuration.GetSection("HttpsCertificateSettings:HttpsInlineCertStore:Certificate:ThumbPrint").Get<string>());
                        }
                    }

                    if (serv_cert != null)
                        options.Listen(ip, hostContext.Configuration.GetSection("ServerConfigurations:SSLPort").Get<int>(), listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http2;
                            listenOptions.UseHttps((httpsOptions) =>
                            {
                                //https://stackoverflow.com/questions/54057615/how-to-use-client-ssl-certificates-with-net-core
                                httpsOptions.ServerCertificate = serv_cert;
                                httpsOptions.ClientCertificateMode = ClientCertificateMode.NoCertificate;
                                // next - client certificate authentication
                                /*
                                httpsOptions.ClientCertificateMode = ClientCertificateMode.AllowCertificate;
                                httpsOptions.CheckCertificateRevocation = false;
                                httpsOptions.ClientCertificateValidation = (cert, validationChain, policyErrors) =>
                                {
                                    // this is for testing non production certificates, do not use these settings in production
                                    //validationChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
                                    //validationChain.ChainPolicy.RevocationFlag = X509RevocationFlag.ExcludeRoot;
                                    //validationChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
                                    //validationChain.ChainPolicy.VerificationTime = DateTime.Now;
                                    //validationChain.ChainPolicy.UrlRetrievalTimeout = new TimeSpan(0, 0, 0);
                                    //validationChain.ChainPolicy.ExtraStore.Add(serv_cert);

                                    var valid = validationChain.Build(cert);
                                    if (!valid) return false;

                                    // only trust certs that are signed by our CA cert
                                    valid = validationChain
                                        .ChainElements
                                        .Cast<X509ChainElement>()
                                        .Any(x => x.Certificate.Thumbprint.Equals(serv_cert.Thumbprint, StringComparison.InvariantCultureIgnoreCase));

                                    return valid;
                                };
                                */
                            });
                        });
                    if (hostContext.Configuration.GetSection("ServerConfigurations:Port").Exists() && !string.IsNullOrWhiteSpace(hostContext.Configuration.GetSection("ServerConfigurations:Port").Value))
                        options.Listen(ip, hostContext.Configuration.GetSection("ServerConfigurations:Port").Get<int>(), listenOptions =>
                        {
                            listenOptions.Protocols = HttpProtocols.Http1;
                        });
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddTransient<IFileLogger>(s => new FileLogger(hostContext.Configuration, pathToContentRoot));
                })
                .UseStartup<Startup>()
                .UseEnvironment(isService ? Environments.Production : Environments.Development);

            if (isService)
            {
                await host.RunAsServiceAsync();//host.RunAsCustomService();
            }
            else
            {
                await host.Build().RunAsync();//host.Run();
            }
        }

        private static X509Certificate2 GetCertificateFromStore(string storeName, StoreLocation storeLocation, string certThumbPrint)
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

        private static X509Certificate2 GetCertificateFromFile(string path, string password)
        {
            return new X509Certificate2(path, password);
        }
    }
}
