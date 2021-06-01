using gRPCNet.Client.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace gRPCNet.Client
{
    public class TcpHostedService : IHostedService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TcpHostedService> _logger;
        private readonly IFileLogger _fileLogger;

        private readonly MessageServer _server;

        public TcpHostedService(
            IServiceProvider serviceProvider,
            IConfiguration configuration,
            ILogger<TcpHostedService> logger,
            IFileLogger fileLogger)
        {
            _serviceProvider = serviceProvider;
            _configuration = configuration;
            _logger = logger;
            _fileLogger = fileLogger;

            string ip = _configuration.GetSection("AppSettings").GetValue<string>("TcpServerIP");
            int port = _configuration.GetSection("AppSettings").GetValue<int>("TcpServerPort");

            _server = new MessageServer(IPAddress.Parse(ip), port, _serviceProvider, _logger, _fileLogger);
        }

        /// <summary>
        /// [implementation of IHostedService] Triggered when the application host is ready to start the service.
        /// StartAsync contains the logic to start the background task. StartAsync is called:
        /// - BEFORE The app's request processing pipeline is configured (Startup.Configure).
        /// - BEFORE The server is started and IApplicationLifetime.ApplicationStarted is triggered.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted</param>
        /// <returns>Task</returns>
        public Task StartAsync(CancellationToken cancellationToken)
        {

            if (_server != null && !_server.IsStarted)
            {
                if (_server.Start())
                {
                    _logger.LogInformation($"TcpServer STARTED. Now listening on: {_server.Endpoint.Address}:{_server.Endpoint.Port}");
                    _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} TcpServer STARTED. Now listening on: {_server.Endpoint.Address}:{_server.Endpoint.Port}");
                }
                else
                {
                    _logger.LogWarning("TcpServer failed to start");
                    _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} TcpServer failed to start");
                }
            }
            
            return Task.CompletedTask;
        }

        /// <summary>
        /// [implementation of IHostedService]
        /// Triggered when the host is performing a graceful shutdown. 
        /// StopAsync contains the logic to end the background task. 
        /// Implement IDisposable and finalizers (destructors) to dispose of any unmanaged resources.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns>Task</returns>
        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_server != null && _server.IsStarted)
            {
                _server.Stop();
                _logger.LogInformation("TcpServer STOPPED");
                _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} TcpServer STOPPED");
            }
            else 
            {
                _logger.LogWarning("TcpServer already stopped");
                _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} TcpServer already stopped");
            }
            
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_server != null && _server.IsStarted && !_server.IsDisposed)
                _server.Dispose();
        }
    }
}