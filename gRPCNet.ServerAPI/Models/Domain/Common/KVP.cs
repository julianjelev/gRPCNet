namespace gRPCNet.ServerAPI.Models.Domain.Common
{
    public class KVP
    {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public string Category { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public bool IsDeleted { get; set; }
    }
}
