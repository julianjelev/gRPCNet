using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpNet.Controller.Models
{
    public class CanPlayRequest
    {
        public string ConcentratorId { get; set; }

        public string GameControllerId { get; set; }

        public string CardType { get; set; }

        public string CardId { get; set; }

        public bool ShoulPay { get; set; }

        public int TransactionId { get; set; }

        public int EndpointRssi { get; set; }

        public int ConcentratorRssi { get; set; }

        public byte[] SerializeASCII() 
        {
            List<byte> sendBytes = new List<byte>();

            // command
            sendBytes.AddRange("RG".Select(c => (byte)c)); // "RG" request game (shoul pay is false), "SG" start game (shoul pay is true) 
            sendBytes.Add(0x1F);// 0x1F - unit separator char(31)
            // concentrator id
            sendBytes.AddRange(ConcentratorId.Select(c => (byte)c));
            sendBytes.Add(0x1F);
            // controller id
            sendBytes.AddRange(GameControllerId.Select(c => (byte)c));
            sendBytes.Add(0x1F);
            // card type
            sendBytes.AddRange(CardType.Select(c => (byte)c));
            sendBytes.Add(0x1F);
            // card id
            sendBytes.AddRange(CardId.Select(c => (byte)c));
            sendBytes.Add(0x1F);
            // should pay
            sendBytes.Add(0x30); //  0x30 '0' char(48)
            sendBytes.Add(0x1F);
            // transaction id
            sendBytes.AddRange(TransactionId.ToString().Select(c => (byte)c));
            sendBytes.Add(0x1F);
            // endpoint rssi
            sendBytes.AddRange(EndpointRssi.ToString().Select(c => (byte)c));
            sendBytes.Add(0x1F);
            // concentrator rssi
            sendBytes.AddRange(ConcentratorRssi.ToString().Select(c => (byte)c));
            sendBytes.Add(0x1E);// 0x1E - record separator char(30)

            return sendBytes.ToArray();
        }
    }
}
