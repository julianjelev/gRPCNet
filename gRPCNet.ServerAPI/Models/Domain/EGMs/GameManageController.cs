using System;
using System.ComponentModel.DataAnnotations;

namespace gRPCNet.ServerAPI.Models.Domain.EGMs
{
    public class GameManageController
    {
        [Key]
        public string Id { get; set; }
        [MaxLength(200)]
        public string ControllerId { get; set; }
        [MaxLength(200)]
        public string SoftwareVersion { get; set; }
        public DateTime ActivatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
        [MaxLength(200)]
        public string PrimaryIpAddress { get; set; }
        [MaxLength(200)]
        public string SecondaryIpAddress { get; set; }
        public bool IsActive { get; set; }
        // electronic game machine
        public string EGMId { get; set; }
        public virtual EGM EGM { get; set; }
    }
}
