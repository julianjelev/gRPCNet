using gRPCNet.ServerAPI.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gRPCNet.ServerAPI.Models.Domain.Common
{
    public class ScheduleHour
    {
        [Key]
        public string Id { get; set; }
        public int StartFromHour { get; set; }
        public int StartFromMinutes { get; set; }
        public int EndAtHour { get; set; }
        public int EndAtMinutes { get; set; }
        public bool IsActive { get; set; }
        public PriceChangerEnum PriceChangerSign { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceModifier { get; set; }
        public string ScheduleDayId { get; set; }
        public virtual ScheduleDay ScheduleDay { get; set; }
    }
}
