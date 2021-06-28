using gRPCNet.ServerAPI.Models.Abstractions;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gRPCNet.ServerAPI.Models.Domain.EGMs
{
    public class LightingSignal : BaseModel
    {
        public LightingSignal()
        {
            Diodes = new List<LightingDiode>();
        }

        [Key]
        public int Id { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        public int Type { get; set; }
        public int Seconds { get; set; }
        public int Period { get; set; }
        public int TextShowTime { get; set; }
        public IList<LightingDiode> Diodes { get; set; }
    }
}
