using gRPCNet.ServerAPI.Models.Domain.Cards;
using gRPCNet.ServerAPI.Models.Domain.EGMs;
using System.ComponentModel.DataAnnotations;

namespace gRPCNet.ServerAPI.Models.Domain.Common
{
    public class AllowedCurrency
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public int Order { get; set; }
        public string EgmId { get; set; }
        public virtual EGM EGM { get; set; }
        public int? CardProfileId { get; set; }
        public CardProfile CardProfile { get; set; }
    }
}
