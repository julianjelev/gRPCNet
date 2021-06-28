using System;
using System.ComponentModel.DataAnnotations;

namespace gRPCNet.ServerAPI.Models.Domain.Places
{
    public class Concentrator
    {
        [Key]
        public string Id { get; set; }
        public string GameCenterId { get; set; }
        public string OwnerId { get; set; }
        [MaxLength(200)]
        public string DeviceId { get; set; }
        [MaxLength(200)]
        public string DeviceIpAddress { get; set; }
        [MaxLength(200)]
        public string DeviceGateway { get; set; }
        [MaxLength(200)]
        public string DeviceMask { get; set; }
        [MaxLength(200)]
        public string SoftwareVersion { get; set; }
        public DateTime ActivatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        [MaxLength(200)]
        public string PrimaryIpAddress { get; set; }
        [MaxLength(200)]
        public string SecondaryIpAddress { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }

        public virtual GameCenter GameCenter { get; set; }
    }
}
