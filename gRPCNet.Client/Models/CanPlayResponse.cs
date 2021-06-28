using System.Text;

namespace gRPCNet.Client.Models
{
    public class CanPlayResponse
    {
        public bool Success { get; set; }
        public string ConcentratorId { get; set; }
        public string GameControllerId { get; set; }
        public string CardId { get; set; }
        public int TransactionId { get; set; }
        public string EGMId { get; set; }
        public string EGMName { get; set; }
        public bool Permission { get; set; }
        public string RelayType { get; set; }
        public int? RelayPulse { get; set; }
        public int? RelayOnTime { get; set; }
        public int? RelayOffTime { get; set; }
        public int? RelayDisplayTime { get; set; }
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

            sb.Append(!string.IsNullOrEmpty(EGMName) ? EGMName : "\x20");
            sb.Append((char)0x1F);

            sb.Append(Permission ? 1 : 0);
            sb.Append((char)0x1F);
            
            sb.Append(!string.IsNullOrEmpty(RelayType) ? RelayType : "\x20");
            sb.Append((char)0x1F);
            
            sb.Append(RelayPulse ?? 0);
            sb.Append((char)0x1F);
            
            sb.Append(RelayOnTime ?? 0);
            sb.Append((char)0x1F);
            
            sb.Append(RelayOffTime ?? 0);
            sb.Append((char)0x1F);
            
            sb.Append(RelayDisplayTime ?? 0);
            sb.Append((char)0x1F);
            
            sb.Append(!string.IsNullOrEmpty(MessageLine1) ? MessageLine1 : "\x20");
            sb.Append((char)0x1F);
            
            sb.Append(!string.IsNullOrEmpty(MessageLine2) ? MessageLine2 : "\x20");
            sb.Append((char)0x1E); // 0x1E - record separator char(30)
            
            return Encoding.ASCII.GetBytes(sb.ToString());
        }

        public static CanPlayResponse FromProtoCanPlayResponse(Proto.CanPlayResponse response) 
        {
            return new CanPlayResponse
            {
                Success = response.ResponseCode == 0,
                ConcentratorId = response.ConcentratorId,
                GameControllerId = response.ControllerId,
                CardId = response.CardId,
                TransactionId = response.TransactionId,
                EGMId = response.ServiceId,
                EGMName = response.ServiceName,
                Permission = response.Permission,
                RelayType = response.RelayType,
                RelayPulse = response.RelayPulse,
                RelayOnTime = response.RelayOnTime,
                RelayOffTime = response.RelayOffTime,
                RelayDisplayTime = response.RelayDisplayTime,
                MessageLine1 = response.ResponseCode == 0 ? response.DisplayLine1 : $"Err {response.ResponseCode}",
                MessageLine2 = response.ResponseCode == 0 ? response.DisplayLine2 : ""
            };
        }
    }
}
