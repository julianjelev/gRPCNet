using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace gRPCNet.ServerAPI.Models.Domain.Common
{
    public class CurrencyTemplate
    {
        public CurrencyTemplate()
        {

        }
        public CurrencyTemplate(decimal balance = 0)
        {
            Balance = balance;
        }

        [Key]
        public string Id { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; private set; }
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
        [Required]
        [MaxLength(100)]
        public string OwnerId { get; set; }
        [Required]
        [MaxLength(500)]
        public string OwnerFullName { get; set; }
    }
}
