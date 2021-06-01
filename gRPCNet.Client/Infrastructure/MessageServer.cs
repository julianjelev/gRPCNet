using gRPCNet.Client.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;

namespace gRPCNet.Client.Infrastructure
{
    public class MessageServer : TcpServerBase
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TcpHostedService> _logger;
        private readonly IFileLogger _fileLogger;

        public MessageServer(
            IPAddress address, 
            int port,
            IServiceProvider serviceProvider,
            ILogger<TcpHostedService> logger,
            IFileLogger fileLogger) : base(address, port)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _fileLogger = fileLogger;
        }

        protected override TcpSessionBase CreateSession()
        {
            ITextMessageService messageService = _serviceProvider.GetService<ITextMessageService>();
            return new MessageSession(this, messageService, _logger, _fileLogger);
        }

        protected override void OnError(SocketError error)
        {
            _logger.LogError($"ERROR MessageServer->OnError: {error}");
            _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR MessageServer->OnError: {error}");
        }
    }
}
