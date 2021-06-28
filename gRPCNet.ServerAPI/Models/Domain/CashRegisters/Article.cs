using gRPCNet.ServerAPI.Models.Abstractions;
using gRPCNet.ServerAPI.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace gRPCNet.ServerAPI.Models.Domain.CashRegisters
{
    public class Article : IOwnerable, IDateable
    {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public bool IsActive { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public ArticleTypeEnum Type { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal DepositValue { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BonusDepositValue { get; set; }
        public bool IsTicketsAllowed { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PriceInTickets { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal OnBuyBonusInTickets { get; set; }
        public bool HasBonusPack { get; set; }
        public int? BonusPackId { get; set; }
        public virtual BonusPack BonusPack { get; set; }
        public int ArticleCategoryId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }

        public virtual IList<PlacesArticles> Places { get; set; }
        public virtual ArticleCategory ArticleCategory { get; set; }
    }
}
