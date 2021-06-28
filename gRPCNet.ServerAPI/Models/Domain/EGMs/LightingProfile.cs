using gRPCNet.ServerAPI.Models.Domain.Common;

namespace gRPCNet.ServerAPI.Models.Domain.EGMs
{
    public class LightingProfile
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsActive { get; set; }
        public string OwnerId { get; set; }
        public int? CommandId { get; set; }
        public virtual KVP Command { get; set; }
        public int SubCommandId { get; set; }
        public virtual KVP SubCommand { get; set; }
        public int? ColorId { get; set; }
        public virtual KVP Color { get; set; }
        public int? ValueId { get; set; }
        public virtual KVP Value { get; set; }
        public int? ValueTypeId { get; set; }
        public virtual KVP ValueType { get; set; }
    }
}
