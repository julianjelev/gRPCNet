﻿using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Client;
using System;
using System.Collections.Generic;
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
            _keepaliveService = _chanelService.Channel != null ? 
                _chanelService.Channel.CreateGrpcService<Proto.IKeepaliveService>() : null;
        }

        /// <summary>
        /// [implementation of IHostedService] Triggered when the application host is ready to start the service.
        /// StartAsync contains the logic to start the background task. StartAsync is called:
        /// - BEFORE The app's request processing pipeline is configured (Startup.Configure).
        /// - BEFORE The server is started and IApplicationLifetime.ApplicationStarted is triggered.
        /// </summary>
        /// <param name="cancellationToken">Indicates that the start process has been aborted</param>
        /// <returns>Task</returns>
        public Task StartAsync(CancellationToken token)
        {
            if (_keepaliveService != null) 
            {
                // Perform post - startup activities here
                Task.Run(async () =>
                {
                    _logger.LogInformation("KeepaliveHostedService STARTED");
                    _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} KeepaliveHostedService STARTED");
                    try
                    {
                        var cc = new CallContext(new CallOptions(cancellationToken: _appLifetime.ApplicationStopping));
                        await foreach (var tic in _keepaliveService.SubscribeAsync(cc))
                        {
                            _logger.LogInformation($"{tic.Time} server respond for client {tic.Client}");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"KeepaliveHostedService->OnStarted->Exception: {ex}");
                        _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} KeepaliveHostedService->OnStarted->Exception: {ex}");
                        if (_chanelService.Channel != null) _chanelService.Channel.Dispose();
                    }
                }, _appLifetime.ApplicationStopping);
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
        public Task StopAsync(CancellationToken token)
        {
            if (_keepaliveService != null) 
            {
                try
                {
                    if (_chanelService.Channel != null) _chanelService.Channel.ShutdownAsync().Wait(token);
                    _logger.LogInformation("KeepaliveHostedService STOPPED");
                    _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} KeepaliveHostedService STOPPED");
                }
                catch (Exception e)
                {
                    if (e is ObjectDisposedException || e is AggregateException)
                    {
                        _logger.LogError($"ERROR KeepaliveHostedService->StopAsync->Exception: {e}");
                        _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR KeepaliveHostedService->StopAsync->Exception: {e}");
                    }
                }
            }
            
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            if (_chanelService.Channel != null) _chanelService.Channel.Dispose();
        }

        /*
         * 
        public Task StartAsync(CancellationToken token)
        {
            _appLifetime.ApplicationStarted.Register(OnStarted, true);
            _appLifetime.ApplicationStopping.Register(OnStopping, true);
            _appLifetime.ApplicationStopped.Register(OnStopped, true);

            return Task.CompletedTask;
        }

        private void OnStarted() 
        {
            // Perform post - startup activities here
            Task.Run(async () => 
            {
                _logger.LogInformation("KeepaliveHostedService STARTED");
                _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} KeepaliveHostedService STARTED");
                try
                {
                    var cc = new CallContext(new CallOptions(cancellationToken: _appLifetime.ApplicationStopping));
                    await foreach (var tic in _keepaliveService.SubscribeAsync(cc))
                    {
                        _logger.LogInformation($"{tic.Time} server respond for client {tic.Client}");
                    }
                }
                catch (Exception ex) 
                {
                    _logger.LogError($"KeepaliveHostedService->OnStarted->Exception: {ex}");
                    _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} KeepaliveHostedService->OnStarted->Exception: {ex}");
                    _chanelService.Channel.Dispose();
                }
            }, _appLifetime.ApplicationStopping);
        }

        private void OnStopping() 
        {
            // Perform on-stopping activities here
            Task.Run(() => 
            {
                try
                {
                    _chanelService.Channel.ShutdownAsync().Wait();
                    _logger.LogInformation("KeepaliveHostedService STOPPING");
                    _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} KeepaliveHostedService STOPPING");
                }
                catch (Exception e)
                {
                    if (e is ObjectDisposedException || e is AggregateException)
                    {
                        _logger.LogError($"ERROR KeepaliveHostedService->OnStopping->Exception: {e}");
                        _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR KeepaliveHostedService->OnStopping->Exception: {e}");
                    }
                }
            });
        }

        private void OnStopped() 
        {
            // Perform post-stopped activities here
            Task.Run(() => 
            {
                _logger.LogInformation("KeepaliveHostedService STOPED");
                _fileLogger.WriteProgramLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} KeepaliveHostedService STOPED");
            });
        }
        */
    }
}
