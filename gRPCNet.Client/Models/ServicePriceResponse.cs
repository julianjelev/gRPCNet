using System.Text;

namespace gRPCNet.Client.Models
{
    public class ServicePriceResponse
    {
        public bool Success { get; set; }
        public string ConcentratorId { get; set; }
        public string GameControllerId { get; set; }
        public string CardId { get; set; }
        public int TransactionId { get; set; }
        public string EGMId { get; set; }
        public string MessageLine1 { get; set; }
        public string MessageLine2 { get; set; }

        public byte[] SerializeASCII() 
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(Success ? 1 : 0);
            sb.Append((char)0x1F);

            sb.Append(!string.IsNullOrEmpty(ConcentratorId) ? ConcentratorId : "\x20");
            sb.Append((char)0x1F); // 0x1F - unit separator char(31)

            sb.Append(!string.IsNullOrEmpty(GameControllerId) ? GameControllerId : "\x20");
            sb.Append((char)0x1F);

            sb.Append(!string.IsNullOrEmpty(CardId) ? CardId : "\x20");
            sb.Append((char)0x1F);

            sb.Append(TransactionId);
            sb.Append((char)0x1F);

            sb.Append(!string.IsNullOrEmpty(EGMId) ? EGMId : "\x20");
            sb.Append((char)0x1F);

            sb.Append(!string.IsNullOrEmpty(MessageLine1) ? MessageLine1 : "\x20");
            sb.Append((char)0x1F);

            sb.Append(!string.IsNullOrEmpty(MessageLine2) ? MessageLine2 : "\x20");
            sb.Append((char)0x1E); // 0x1E - record separator char(30)

            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        public static ServicePriceResponse FromProtoServicePriceResponse(Proto.ServicePriceResponse response)
        {
            return new ServicePriceResponse 
            {
                Success = response.ResponseCode == 0,
                ConcentratorId = response.ConcentratorId,
                GameControllerId = response.ControllerId,
                CardId = response.CardId,
                TransactionId = response.TransactionId,
                EGMId = response.ServiceId,
                MessageLine1 = response.ResponseCode == 0 ? response.ServiceName : $"Err {response.ResponseCode}",
                MessageLine2 = response.ResponseCode == 0 ? $"{response.Price:F2}" : "",
            };
        }
    }
}
