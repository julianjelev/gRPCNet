using gRPCNet.ServerAPI.Models.Domain.Common;
using System;
using System.Collections.Generic;

namespace gRPCNet.ServerAPI.Models.Domain.Cards
{
    public class CardProfile
    {
        public int Id { get; set; }
        public bool IsActive { get; set; }
        public string ConfigRelayId { get; set; }
        public string Serie { get; set; }
        public DateTime ActiveFrom { get; set; }
        public DateTime ExpirationOn { get; set; }
        public string State { get; set; }
        public string Mode { get; set; }
        public string Type { get; set; }
        public string PrintedNumber { get; set; }

        public IList<AllowedCurrency> Currencies { get; set; }
    }
}
