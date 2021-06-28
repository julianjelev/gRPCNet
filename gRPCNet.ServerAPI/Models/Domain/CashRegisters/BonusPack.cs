using System;
using System.Collections.Generic;

namespace gRPCNet.ServerAPI.Models.Domain.CashRegisters
{
    public class BonusPack
    {
        public BonusPack()
        {
            Articles = new List<Article>();
        }
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public string Name { get; set; }
        public string ForeignKey { get; set; }
        public bool HasProduct { get; set; }
        public bool AllowPartialUse { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }

        public virtual IList<Article> Articles { get; set; }
    }
}
