using gRPCNet.ServerAPI.Models.Domain.Places;

namespace gRPCNet.ServerAPI.Models.Domain.Users
{
    public class UserPlace
    {
        public string UserId { get; set; }
        public string GameCenterId { get; set; }

        public virtual User User { get; set; }
        public virtual GameCenter GameCenter { get; set; }
    }
}
