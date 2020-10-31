using gRPCNet.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
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
                TcpClient client = default;
                try
                {
                    _server.Start();
                    _logger.LogInformation("TcpServer STARTED");
                    _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} TcpServer STARTED");

                    while (true)
                    {
                        /// AcceptTcpClient is a blocking method that returns a TcpClient that you can use to send and receive data. 
                        client = _server.AcceptTcpClient();

                        TcpClientHandler clientHandler = new TcpClientHandler(client, _serviceProvider, _logger, _fileLogger);
                        clientHandler.HandleRequest();
                    }
                }
                catch (SocketException ex1)
                {
                    if (ex1.SocketErrorCode == SocketError.Interrupted)
                        _logger.LogWarning("A blocking listen has been cancelled");
                    else
                    {
                        _logger.LogError($"ERROR TcpServer->OnStarted->SocketException: {ex1}");
                        _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR TcpServer->OnStarted->SocketException: {ex1}");
                    }
                }
                catch (InvalidOperationException ex2)
                {
                    _logger.LogError($"ERROR TcpServer->OnStarted->InvalidOperationException: {ex2}");
                    _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR TcpServer->OnStarted->InvalidOperationException: {ex2}");   
                }
                
                _server.Server.Dispose();
            });
        }
        private void OnStopping() 
        {
            // Perform on-stopping activities here
            Task.Run(() => 
            {
                try
                {
                    _server.Stop();
                    _logger.LogInformation("TcpServer STOPPING ...");
                    _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} TcpServer STOPPING");
                }
                catch (SocketException e)
                {
                    _logger.LogError($"ERROR TcpServer->OnStopping->SocketException: {e}");
                    _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR TcpServer->OnStopping->SocketException: {e}");
                }
            });
        }
        private void OnStopped() 
        {
            // Perform post-stopped activities here
            Task.Run(() => 
            {
                _logger.LogInformation("TcpServer STOPPED");
            });
        }
    }

    //Class to handle each client request separatly
    public class TcpClientHandler 
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TcpHostedService> _logger;
        private readonly IFileLogger _fileLogger;

        private readonly TcpClient _client;

        public TcpClientHandler(
            TcpClient client,
            IServiceProvider serviceProvider,
            ILogger<TcpHostedService> logger,
            IFileLogger fileLogger)
        {
            _client = client;
            _serviceProvider = serviceProvider;
            _logger = logger;
            _fileLogger = fileLogger;
        }

        public void HandleRequest() 
        {
            Thread thread = new Thread(HandleRequestInternal);
            thread.Start();
        }

        private void HandleRequestInternal() 
        {
            byte[] inputBuffer = new byte[_client.ReceiveBufferSize];

            while (true)
            {
                try 
                {
                    if (!_client.Connected) 
                        break;

                    NetworkStream stream = _client.GetStream();
                    if (!stream.CanRead) 
                        break;

                    if (!stream.DataAvailable)
                        Thread.Sleep(1);// Give up the remaining time slice.
                    else 
                    {
                        int numberOfBytesRead = stream.Read(inputBuffer, 0, _client.ReceiveBufferSize);

                        byte[] outputBuffer = numberOfBytesRead > 0 ? 
                            _serviceProvider
                                .GetService<ISocketMessageService>()
                                .ProccessRequest(inputBuffer
                                    .Take(numberOfBytesRead)
                                    .ToArray()) : new byte[0];

                        if (outputBuffer != null && outputBuffer.Length > 0)
                            stream.Write(outputBuffer, 0, outputBuffer.Length);
                        else
                            stream.Write(BitConverter.GetBytes(false));

                        stream.Flush();
                    }
                }
                catch (Exception e) 
                {
                    _logger.LogError($"ERROR TcpClientHandler->HandleRequestInternal: {e}");
                    _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR TcpClientHandler->HandleRequestInternal: {e}");
                    break;
                }
            }
        }
    }
}
