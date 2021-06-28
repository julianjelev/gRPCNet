using gRPCNet.ServerAPI.Models.Domain.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gRPCNet.ServerAPI.Models.Domain.EGMs
{
    public class ControllerConfigRelay
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("EGM")]
        public string EGMId { get; set; }
        public virtual EGM EGM { get; set; }
        [MaxLength(200)]
        public string InternalIp { get; set; }
        public int MinTransactionTime { get; set; }
        public string SerialNumber { get; set; }
        public string OwnerId { get; set; }
        public string GameCenterId { get; set; }
        public string CashRegisterId { get; set; }
        public string ForeignKeyId { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string CreatedBy { get; set; }
        public string ModifiedBy { get; set; }
        public bool IsDeleted { get; set; }
        public int OnSuccessLightingSignal { get; set; }
        public int OnErrorLightingSignal { get; set; }
        public int OnActionLightingSignal { get; set; }
        public int OnInfoLightingSignal { get; set; }
        public int UpdateTime { get; set; }
        public string ConfigRelayTypeId { get; set; }
        public virtual KVP ConfigRelayType { get; set; }
        public string IdleLineOne { get; set; }
        public string IdleLineTwo { get; set; }
        public string IdleLineThree { get; set; }
        public string IdleLineFour { get; set; }
        public string PlayLineOne { get; set; }
        public string PlayLineTwo { get; set; }
        public string PlayLineThree { get; set; }
        public string PlayLineFour { get; set; }
        public string RejectLineOne { get; set; }
        public string RejectLineTwo { get; set; }
        public string RejectLineThree { get; set; }
        public string RejectLineFour { get; set; }
        public string Firmware { get; set; }

        public override string ToString()
        {
            return $"Serial Number: {this.SerialNumber} | Internal IP: {this.InternalIp} | Min Tx Time: {this.MinTransactionTime}";
        }
    }
}
