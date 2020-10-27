using gRPCNet.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace gRPCNet.Client
{
    public class TcpHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TcpHostedService> _logger;
        private readonly IFileLogger _fileLogger;

        private CancellationTokenSource _cancellationTokenSource;
        private readonly TcpListener _server;

        public TcpHostedService(
            IServiceProvider serviceProvider,
            IHostApplicationLifetime appLifetime,
            IConfiguration configuration,
            ILogger<TcpHostedService> logger,
            IFileLogger fileLogger)
        {
            _serviceProvider = serviceProvider;
            _appLifetime = appLifetime;
            _configuration = configuration;
            _logger = logger;
            _fileLogger = fileLogger;

            string ip = _configuration.GetSection("AppSettings").GetValue<string>("TcpServerIP");
            int port = _configuration.GetSection("AppSettings").GetValue<int>("TcpServerPort");

            _cancellationTokenSource = new CancellationTokenSource();
            _server = new TcpListener(IPAddress.Parse(ip), port);
        }

        public void Dispose()
        {
            _server.Server.Dispose();
        }

        /// <summary>
        /// [implementation of IHostedService] Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted</param>
        /// <returns>Task</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }

        /// <summary>
        /// [implementation of IHostedService] Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <returns>Task</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        private void OnStarted() 
        {
            // Perform post - startup activities here
            Task.Run(() =>
            {
                try
                {
                    _server.Start();
                    _logger.LogInformation("TcpServer STARTED");
                    _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} TcpServer STARTED");
                    while (true)
                    {
                        //_fileLogger.WriteProgramLog($"TcpServer waiting for a connection...");

                        /// AcceptTcpClient is a blocking method that returns a TcpClient that you can use to send and receive data. 
                        TcpClient client = _server.AcceptTcpClient();

                        //_fileLogger.WriteProgramLog($"TcpServer client is connected!");

                        Thread t = new Thread(new ParameterizedThreadStart(HandleRequest));
                        t.Start(client);
                    }
                }
                catch (SocketException e)
                {
                    _logger.LogError($"ERROR TcpServer->OnStarted->SocketException: {e}");
                    _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR TcpServer->OnStarted->SocketException: {e}");
                }
            });
        }
        private void OnStopping() 
        {
            // Perform on-stopping activities here
            _logger.LogInformation("TcpServer STOPPING ...");
            _cancellationTokenSource.Cancel();
        }
        private void OnStopped() 
        {
            try
            {
                _server.Stop();
                _logger.LogInformation("TcpServer STOPED");
                _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} TcpServer STOPED");
            }
            catch (SocketException e)
            {
                _logger.LogError($"ERROR TcpServer->OnStopped->SocketException: {e}");
                _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR TcpServer->OnStopped->SocketException: {e}");
            }
        }

        private void HandleRequest(object obj) 
        {
            TcpClient client = (TcpClient)obj;
            NetworkStream stream = client.GetStream();

            try 
            {
                if (client.Connected && stream != null && stream.CanRead) 
                {
                    byte[] inputBuffer = new byte[client.ReceiveBufferSize];
                    int numberOfBytesRead = 0;
                    if (stream.DataAvailable) 
                    {
                        numberOfBytesRead = stream.Read(inputBuffer, 0, inputBuffer.Length);
                    }
                    byte[] outputBuffer = null;
                    using (var scope = _serviceProvider.CreateScope()) 
                    {
                        var services = _serviceProvider.GetService<ISocketMessageService>();
                        outputBuffer = services.ProccessRequest(inputBuffer);
                    }
                    if (outputBuffer != null && outputBuffer.Length > 0)
                        stream.Write(outputBuffer, 0, outputBuffer.Length);
                    else
                        stream.Write(BitConverter.GetBytes(false));
                }
            }
            catch (Exception e)
            {
                _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR TcpServer->HandleRequest: {e}");
            }
            finally
            {
                stream.Close();
                client.Close();
            }
        }
    }
}
