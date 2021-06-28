namespace gRPCNet.ServerAPI.Models.Domain.EGMs
{
    public class LightingDiode
    {
        public int Id { get; set; }
        public int Order { get; set; }
        public byte RedValue { get; set; }
        public byte GreenValue { get; set; }
        public byte BlueValue { get; set; }
        public int LightingSignalId { get; set; }
        public virtual LightingSignal LightingSignal { get; set; }
    }
}
