using gRPCNet.ServerAPI.Models.Domain.Places;

namespace gRPCNet.ServerAPI.Models.Domain.CashRegisters
{
    public class PlacesArticles
    {
        public string GameCenterId { get; set; }
        public int ArticleId { get; set; }

        public virtual GameCenter GameCenter { get; set; }
        public virtual Article Article { get; set; }
    }
}
