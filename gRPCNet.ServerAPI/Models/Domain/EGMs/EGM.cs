using gRPCNet.ServerAPI.Models.Domain.Common;
using gRPCNet.ServerAPI.Models.Domain.Places;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gRPCNet.ServerAPI.Models.Domain.EGMs
{
    public class EGM
    {
        public EGM()
        {
            LightingSignals = new List<LightingSignal>();
            GameManageControllers = new List<GameManageController>();
            ScheduleDays = new List<ScheduleDay>();
            AllowedCurrencies = new List<AllowedCurrency>();
            Images = new List<ImageInfo>();
            ConfigRelays = new List<ControllerConfigRelay>();
        }

        [Key]
        public string Id { get; set; }
        [MaxLength(200)]
        public string SerialNumber { get; set; }
        [MaxLength(200)]
        public string MachineName { get; set; }
        [MaxLength(200)]
        public string ManufacturerName { get; set; }
        [MaxLength(200)]
        public string Version { get; set; }
        public DateTime ActivatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PricePerGame { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BonusAfterPlay { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BonusOnWin { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BonusOnline { get; set; }
        public bool IsActive { get; set; }
        public string GameCenterId { get; set; }
        public string OwnerId { get; set; }
        public int GameCategoryId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        /* removed from controller config relay */
        public string RelayMode { get; set; }
        public int RelayPulse { get; set; }
        public int RelayOnTime { get; set; }
        public int RelayOffTime { get; set; }
        public string RelayFeedBack { get; set; }
        public int RelayDisplayTime { get; set; }
        public int MinTransactionTime { get; set; }
        public bool IsDeleted { get; set; }
        public int? LightingEventId { get; set; }

        public virtual GameCenter GameCenter { get; set; }
        public GameCategory GameCategory { get; set; }
        public virtual LightingEvent LightingEvent { get; set; }
        public virtual IList<LightingSignal> LightingSignals { get; set; }
        public virtual IList<GameManageController> GameManageControllers { get; set; }
        public virtual IList<ScheduleDay> ScheduleDays { get; set; }
        public virtual IList<AllowedCurrency> AllowedCurrencies { get; set; }
        public virtual IList<ImageInfo> Images { get; set; }
        public virtual IList<ControllerConfigRelay> ConfigRelays { get; set; }
    }
}
