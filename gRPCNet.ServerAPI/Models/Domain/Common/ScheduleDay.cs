using gRPCNet.ServerAPI.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gRPCNet.ServerAPI.Models.Domain.Common
{
    public class ScheduleDay
    {
        public ScheduleDay()
        {
            Hours = new List<ScheduleHour>();
        }
        [Key]
        public string Id { get; set; }
        public bool IsActive { get; set; }
        public int DayOfWeek { get; set; }
        public bool IsDateRange { get; set; }
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public PriceChangerEnum PriceChangerSign { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceModifier { get; set; }
        public string EGMId { get; set; }
        public string GameCenterId { get; set; }
        public string OwnerId { get; set; }
        public virtual IList<ScheduleHour> Hours { get; set; }
    }
}
