using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TcpNet.Controller.Models
{
    public class CanPlaySuccessResponse
    {
        public string ConcentratorId { get; set; }
        public string GameControllerId { get; set; }
        public string CardId { get; set; }
        public int TransactionId { get; set; }
        public string GameId { get; set; }
        public bool Permission { get; set; }
        public string RelayType { get; set; }
        public int? RelayPulse { get; set; }
        public int? RelayOnTime { get; set; }
        public int? RelayOffTime { get; set; }
        public int? RelayDisplayTime { get; set; }
        public string MessageLine1 { get; set; }
        public string MessageLine2 { get; set; }

        public static CanPlaySuccessResponse DeserializeASCII(byte[][] buffer) 
        {
            // how many tokens
            int tokensCount = buffer.GetLength(0);
            // concentrator id
            char[] p1 = tokensCount >= 1 ?
                buffer[0]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // controller id
            char[] p2 = tokensCount >= 2 ?
                buffer[1]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // card id
            char[] p3 = tokensCount >= 3 ?
                buffer[2]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // transaction
            char[] p4 = tokensCount >= 4 ?
                buffer[3]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // game id
            char[] p5 = tokensCount >= 5 ?
                buffer[4]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // permission
            char[] p6 = tokensCount >= 6 ?
                buffer[5]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // relay type
            char[] p7 = tokensCount >= 7 ?
                buffer[6]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // relay pulse
            char[] p8 = tokensCount >= 8 ?
                buffer[7]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // relay on time
            char[] p9 = tokensCount >= 9 ?
                buffer[8]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // relay off time
            char[] p10 = tokensCount >= 10 ?
                buffer[9]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // relay display time
            char[] p11 = tokensCount >= 11 ?
                buffer[10]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // message line1
            char[] p12 = tokensCount >= 12 ?
                buffer[11]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;

            // message line2
            char[] p13 = tokensCount >= 13 ?
                buffer[12]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;

            CanPlaySuccessResponse response = new CanPlaySuccessResponse
            {
                ConcentratorId = p1 != null ? new string(p1) : string.Empty,
                GameControllerId = p2 != null ? new string(p2) : string.Empty,
                CardId = p3 != null ? new string(p3) : string.Empty,
                TransactionId = p4 != null && p4.Count() > 0 ? int.Parse(new string(p4)) : 0,
                GameId = p5 != null ? new string(p5) : string.Empty,
                Permission = p6 != null && p6.Count() > 0 && p6[0] == '1', //  0x30 '0' char(48)
                RelayType = p7 != null ? new string(p7) : string.Empty,
                RelayPulse = p8 != null && p8.Count() > 0 ? int.Parse(new string(p8)) : 0,
                RelayOnTime = p9 != null && p9.Count() > 0 ? int.Parse(new string(p9)) : 0,
                RelayOffTime = p10 != null && p10.Count() > 0 ? int.Parse(new string(p10)) : 0,
                RelayDisplayTime = p11 != null && p11.Count() > 0 ? int.Parse(new string(p11)) : 0,
                MessageLine1 = p12 != null ? new string(p12) : string.Empty,
                MessageLine2 = p13 != null ? new string(p13) : string.Empty
            };

            return response;
        }
    }
}
