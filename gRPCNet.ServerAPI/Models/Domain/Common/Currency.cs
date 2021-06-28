using gRPCNet.ServerAPI.Models.Domain.Cards;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gRPCNet.ServerAPI.Models.Domain.Common
{
    public class Currency
    {
        public Currency()
        {

        }

        public Currency(decimal primaryBalance = 0, decimal bonusBalance = 0)
        {
            PrimaryBalance = primaryBalance;
            BonusBalance = bonusBalance;
        }

        [Key]
        public string Id { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal PrimaryBalance { get; private set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal BonusBalance { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public string Description { get; set; }
        [Required]
        [MaxLength(100)]
        public string Type { get; set; }
        public bool IsPrimary { get; set; }
        public bool IsPayable { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public int Order { get; set; }
        public string Code { get; set; }
        public string CardId { get; set; }
        [Required]
        [MaxLength(100)]
        public string OwnerId { get; set; }
        [Required]
        [MaxLength(500)]
        public string OwnerFullName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalIncome { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalSpending { get; set; }

        public virtual Card Card { get; set; }

        #region public instance methods

        public decimal GetTotalBalance()
        {
            return PrimaryBalance + BonusBalance;
        }

        #endregion
    }
}
