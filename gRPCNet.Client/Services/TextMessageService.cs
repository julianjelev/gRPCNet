using gRPCNet.Client.Models;
using Microsoft.Extensions.Logging;
using ProtoBuf.Grpc.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace gRPCNet.Client.Services
{
    //По 1 инстанция за всяка 1 сесия.
    public interface ITextMessageService
    {
        byte[] ProccessRequest(byte[] message);
        void StartProcessing();
        void StopProcessing();
    }

    public class TextMessageService : ITextMessageService
    {
        private readonly GrpcChannelService _chanelService;
        private readonly ILogger<TextMessageService> _logger;
        private Proto.ICardService _cardService;

        public TextMessageService(GrpcChannelService chanelService, ILogger<TextMessageService> logger)
        {
            _chanelService = chanelService;
            _logger = logger;
            _cardService = _chanelService.Channel != null ? 
            _chanelService.Channel.CreateGrpcService<Proto.ICardService>() : null;
        }

        public void StartProcessing() 
        {
            
        }

        // метода се извиква при всяко запитване на конектнатия клиент
        public byte[] ProccessRequest(byte[] message)
        {
            //извличане на входните данни
            var splittedMessage = SplitByteArray(message, 0x1F).ToArray();
            if (splittedMessage.Count() == 0) 
                return new byte[] { 0x45, 0x52, 0x52, 0x4F, 0x52 }; //ERROR

            // how many tokens
            int tokensCount = splittedMessage.GetLength(0);
            // command
            string cmd = tokensCount >= 1 ? new string(
                splittedMessage[0]
                    .Where(b => b != 0x1F && b != 0x1E) // except:  0x1F - unit separator char(31), 0x1E - record separator char(30)
                    .Select(b => (char)b)
                    .ToArray()) : string.Empty;

            if (cmd.Equals("RG", StringComparison.InvariantCultureIgnoreCase))
            {
                var request = CanPlayRequest.DeserializeASCII(splittedMessage);

                // to do ... 
                _logger.LogInformation(System.Text.Json.JsonSerializer.Serialize(request));
                //work 
                Thread.Sleep(1000);
                //end work

                var response = new CanPlaySuccessResponse() 
                {
                    ConcentratorId = request.ConcentratorId,
                    GameControllerId = request.GameControllerId,
                    CardId = request.CardId,
                    TransactionId = request.TransactionId,
                    GameId = "",
                    Permission = true,
                    RelayType = "",
                    RelayPulse = 5,
                    RelayOnTime = 10,
                    RelayOffTime = 10,
                    RelayDisplayTime = 30,
                    MessageLine1 = "Price",
                    MessageLine2 = "Credit"
                };
                return response.SerializeASCII();
            }
            else if (cmd.Equals("T", StringComparison.InvariantCultureIgnoreCase))
            {
                return Encoding.ASCII.GetBytes($"{DateTime.Now:dd.MM.yyyy HH:mm}\x1E");
            }
            else
            {
                return Encoding.ASCII.GetBytes($"{DateTime.Now:dd.MM.yyyy HH:mm}\x1E");
            }
        }

        public void StopProcessing() 
        {
            
        }

        #region utils

        private IEnumerable<byte[]> SplitByteArray(IEnumerable<byte> source, byte delimiter) 
        {
            if (source == null) throw new ArgumentNullException("source is null");

            List<byte> current = new List<byte>();

            foreach (byte b in source) 
            {
                if (b == delimiter) 
                {
                    if (current.Count > 0)
                        yield return current.ToArray(); // copies the elements of the List to a new Array

                    current.Clear();
                }
                current.Add(b);
            }

            if (current.Count > 0)
                yield return current.ToArray(); // copies the elements of the List to a new Array
        }

        #endregion
    }
}

/*
 .Select(b => (char)(b + (byte)0x30))    // convert byte to ascii char
 */