namespace gRPCNet.ServerAPI.Models.Domain.Owners
{
    public class UserOwner
    {
        public string UserId { get; set; }
        public virtual Users.User User { get; set; }
        public string OwnerId { get; set; }
        public virtual Owner Owner { get; set; }
    }
}
