using gRPCNet.Client.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Sockets;

namespace gRPCNet.Client.Infrastructure
{
    //По 1 инстанция за всяка 1 конекция. 
    public class MessageSession : TcpSessionBase
    {
        private readonly ITextMessageService _messageService;
        private readonly ILogger<TcpHostedService> _logger;
        private readonly IFileLogger _fileLogger;

        public MessageSession(
            TcpServerBase server,
            ITextMessageService messageService,
            ILogger<TcpHostedService> logger,
            IFileLogger fileLogger) : base(server)
        {
            _logger = logger;
            _fileLogger = fileLogger;
            _messageService = messageService;
        }

        protected override void OnConnected()
        {
            _messageService.StartProcessing();
        }

        // метода се извиква при всяко запитване на конектнатия клиент
        protected override void OnReceived(byte[] inputBuffer, long offset, long size)
        {
            byte[] outputBuffer = size > 0 ?
                _messageService.ProccessRequest(
                    inputBuffer
                        .Take((int)size)
                        .ToArray()) : new byte[0];
            if (outputBuffer != null && outputBuffer.Length > 0)
                Send(outputBuffer);
            else
                Send(BitConverter.GetBytes(false));
        }

        protected override void OnDisconnected()
        {
            _messageService.StopProcessing();
        }

        protected override void OnError(SocketError error)
        {
            _logger.LogError($"ERROR MessageSession->OnError: {error}");
            _fileLogger.WriteErrorLog($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} ERROR MessageSession->OnError: {error}");
        }
    }
}
