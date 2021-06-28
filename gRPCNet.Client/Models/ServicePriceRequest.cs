using System.Linq;

namespace gRPCNet.Client.Models
{
    public class ServicePriceRequest
    {
        public string ConcentratorId { get; set; }

        public string GameControllerId { get; set; }

        public string CardType { get; set; }

        public string CardId { get; set; }

        public string EGMId { get; set; }

        public int TransactionId { get; set; }

        public int EndpointRssi { get; set; }

        public int ConcentratorRssi { get; set; }

        public Proto.ServicePriceRequest ToProtoServicePriceRequest()
        {
            int.TryParse(CardType, out int cardType);
            return new Proto.ServicePriceRequest
            {
                Time = System.DateTime.Now,
                ConcentratorId = ConcentratorId,
                ControllerId = GameControllerId,
                CardType = cardType,
                CardId = CardId,
                ServiceId = EGMId,
                TransactionId = TransactionId,
                EndpointRssi = EndpointRssi,
                ConcentratorRssi = ConcentratorRssi
            };
        }
        public static ServicePriceRequest DeserializeASCII(byte[][] buffer) 
        {
            // how many tokens
            int tokensCount = buffer.GetLength(0);
            // command
            string cmd = tokensCount >= 1 ? new string(
                buffer[0]
                    .Where(b => b != 0x1F && b != 0x1E) // except:  0x1F - unit separator char(31), 0x1E - record separator char(30)
                    .Select(b => (char)b)
                    .ToArray()) : string.Empty;
            // concentrator id
            char[] p1 = tokensCount >= 2 ?
                buffer[1]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // controller id
            char[] p2 = tokensCount >= 3 ?
                buffer[2]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // card type
            char[] p3 = tokensCount >= 4 ?
                buffer[3]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // card id
            char[] p4 = tokensCount >= 5 ?
                buffer[4]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // service id'
            char[] p5 = tokensCount >= 6 ?
                buffer[5]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // transaction id '0'-'255'
            char[] p6 = tokensCount >= 7 ?
                buffer[6]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // endpoint rssi '0'-'255'
            char[] p7 = tokensCount >= 8 ?
                buffer[7]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;
            // concentrator rssi 0-255
            char[] p8 = tokensCount >= 9 ?
                buffer[8]
                    .Where(b => b != 0x1F && b != 0x1E)
                    .Select(b => (char)b)
                    .ToArray() : null;

            ServicePriceRequest request = new ServicePriceRequest
            {
                ConcentratorId = p1 != null ? new string(p1) : string.Empty,
                GameControllerId = p2 != null ? new string(p2) : string.Empty,
                CardType = p3 != null ? new string(p3) : string.Empty,
                CardId = p4 != null ? new string(p4) : string.Empty,
                EGMId = p5 != null ? new string(p5) : string.Empty,
                TransactionId = p6 != null && p6.Count() > 0 ? int.Parse(new string(p6)) : 0,
                EndpointRssi = p7 != null && p7.Count() > 0 ? int.Parse(new string(p7)) : 0,
                ConcentratorRssi = p8 != null && p8.Count() > 0 ? int.Parse(new string(p8)) : 0
            };

            return request;
        }
    }
}
