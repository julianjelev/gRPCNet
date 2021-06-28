using gRPCNet.ServerAPI.Models.Domain.CashRegisters;
using gRPCNet.ServerAPI.Models.Domain.Common;
using gRPCNet.ServerAPI.Models.Domain.EGMs;
using gRPCNet.ServerAPI.Models.Domain.Users;
using gRPCNet.ServerAPI.Models.Dto.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gRPCNet.ServerAPI.Models.Domain.Places
{
    public class GameCenter
    {
        public GameCenter()
        {
            EGMs = new List<EGM>();
            Concentrators = new List<Concentrator>();
            ScheduleDays = new List<ScheduleDay>();
            Articles = new List<PlacesArticles>();
            ConcentratorBoolCheckers = new List<BoolChecker>();
        }

        [Key]
        public string Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        [MaxLength(200)]
        public string CountryId { get; set; }
        public string CountryName { get; set; }

        [MaxLength(200)]
        public string CityId { get; set; }
        public string CityName { get; set; }

        [MaxLength(1000)]
        public string Address { get; set; }

        [MaxLength(200)]
        public string OwnerId { get; set; }

        [MaxLength(200)]
        public string OwnerName { get; set; }

        public bool IsActive { get; set; }

        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }

        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }

        public bool IsDelete { get; set; }

        // ticket machine multiplier
        public double TicketMachineMultiplier { get; set; }
        public DateTime? FromTicketMachineMultiplier { get; set; }
        public DateTime? ToTicketMachineMultiplier { get; set; }
        public bool FromDataToDate { get; set; }
        public byte DayOfWeek { get; set; }

        public virtual IList<EGM> EGMs { get; set; }
        public virtual IList<Concentrator> Concentrators { get; set; }

        public virtual IList<ScheduleDay> ScheduleDays { get; set; }

        public virtual IList<PlacesArticles> Articles { get; set; }
        public virtual IList<UserPlace> Workers { get; set; }

        [NotMapped]
        public IList<ImageInfoDto> Images { get; set; }
        [NotMapped]
        public IList<BoolChecker> ImageEditorCheck { get; set; }
        //[NotMapped]
        //public IList<IFormFile> Files { get; set; }
        [NotMapped]
        public IList<BoolChecker> ConcentratorBoolCheckers { get; set; }
    }
}
