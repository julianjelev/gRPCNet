using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace gRPCNet.Client
{
    public class KeepaliveHostedService : IHostedService, IDisposable
    {
        private readonly GrpcChannelService _chanelService;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ILogger<KeepaliveHostedService> _logger;
        private readonly IFileLogger _fileLogger;

        private CancellationTokenSource _cancellationTokenSource;
        private Proto.IKeepaliveService _keepaliveService;

        public KeepaliveHostedService(
            GrpcChannelService chanelService,
            IHostApplicationLifetime appLifetime,
            ILogger<KeepaliveHostedService> logger,
            IFileLogger fileLogger) 
        {
            _chanelService = chanelService;
            _appLifetime = appLifetime;
            _logger = logger;
            _fileLogger = fileLogger;
            _cancellationTokenSource = new CancellationTokenSource();
            _keepaliveService = _chanelService.Channel.CreateGrpcService<Proto.IKeepaliveService>();
        }

        /// <summary>
        /// [implementation of IHostedService] Triggered when the application host is ready to start the service.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted</param>
        /// <returns>Task</returns>
        public Task StartAsync(CancellationToken token)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted);
            _appLifetime.ApplicationStopping.Register(OnStopping);
            _appLifetime.ApplicationStopped.Register(OnStopped);

            return Task.CompletedTask;
        }
        /// <summary>
        /// [implementation of IHostedService] Triggered when the application host is performing a graceful shutdown.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the shutdown process should no longer be graceful.</param>
        /// <returns>Task</returns>
        public Task StopAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _chanelService.Channel.Dispose();
        }

        //
        private void OnStarted() 
        {
            _logger.LogInformation("KeepaliveHostedService STARTED");
            _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} KeepaliveHostedService STARTED");
            // Perform post - startup activities here
            Task.Run(async () => 
            {
                try
                {
                    var options = new CallOptions(cancellationToken: _cancellationTokenSource.Token);
                    await foreach (var tic in _keepaliveService.SubscribeAsync(new CallContext(options))) 
                    {
                        _logger.LogInformation($"{tic.Time} server respond for client {tic.Client}");
                    }
                }
                catch (RpcException ex) 
                {
                    _logger.LogError($"KeepaliveHostedService->OnStarted->RpcException: {ex}");
                    _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} KeepaliveHostedService->OnStarted->RpcException: {ex}");
                }
            });
        }

        private void OnStopping() 
        {
            // Perform on-stopping activities here
            _logger.LogInformation("KeepaliveHostedService STOPPING");
            _cancellationTokenSource.Cancel();
        }

        private void OnStopped() 
        {
            _logger.LogInformation("KeepaliveHostedService STOPED");
            _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} KeepaliveHostedService STOPED");
            // Perform post-stopped activities here
        }
    }
}
