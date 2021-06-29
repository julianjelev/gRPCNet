//OLD

//using System.Text;

//namespace gRPCNet.Client.Models
//{
//    public class CanPlaySuccessResponse
//    {
//        public string ConcentratorId { get; set; }
//        public string GameControllerId { get; set; }
//        public string CardId { get; set; }
//        public int TransactionId { get; set; }
//        public string GameId { get; set; }
//        public bool Permission { get; set; }
//        public string RelayType { get; set; }
//        public int? RelayPulse { get; set; }
//        public int? RelayOnTime { get; set; }
//        public int? RelayOffTime { get; set; }
//        public int? RelayDisplayTime { get; set; }
//        public string MessageLine1 { get; set; }
//        public string MessageLine2 { get; set; }

//        public byte[] SerializeASCII() 
//        {
//            StringBuilder sb = new StringBuilder();
//            sb.Append(!string.IsNullOrEmpty(ConcentratorId) ? ConcentratorId : "\x20");
//            sb.Append((char)0x1F); // 0x1F - unit separator char(31)

//            sb.Append(!string.IsNullOrEmpty(GameControllerId) ? GameControllerId : "\x20");
//            sb.Append((char)0x1F);
            
//            sb.Append(!string.IsNullOrEmpty(CardId) ? CardId : "\x20");
//            sb.Append((char)0x1F);
            
//            sb.Append(TransactionId);
//            sb.Append((char)0x1F);
            
//            sb.Append(!string.IsNullOrEmpty(GameId) ? GameId : "\x20");
//            sb.Append((char)0x1F);
            
//            sb.Append(Permission ? 1 : 0);
//            sb.Append((char)0x1F);
            
//            sb.Append(!string.IsNullOrEmpty(RelayType) ? RelayType : "\x20");
//            sb.Append((char)0x1F);
            
//            sb.Append(RelayPulse ?? 0);
//            sb.Append((char)0x1F);
            
//            sb.Append(RelayOnTime ?? 0);
//            sb.Append((char)0x1F);
            
//            sb.Append(RelayOffTime ?? 0);
//            sb.Append((char)0x1F);
            
//            sb.Append(RelayDisplayTime ?? 0);
//            sb.Append((char)0x1F);
            
//            sb.Append(!string.IsNullOrEmpty(MessageLine1) ? MessageLine1 : "\x20");
//            sb.Append((char)0x1F);
            
//            sb.Append(!string.IsNullOrEmpty(MessageLine2) ? MessageLine2 : "\x20");
//            sb.Append((char)0x1E); // 0x1E - record separator char(30)
            
//            return Encoding.ASCII.GetBytes(sb.ToString());
//        }
//    }
//}
