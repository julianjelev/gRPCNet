using System;
using System.ComponentModel.DataAnnotations;

namespace gRPCNet.ServerAPI.Models.Domain.Cards
{
    public class CardType
    {
        public string Id { get; set; }
        [MaxLength(100000)]
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        [MaxLength(500)]
        public string Name { get; set; }
        public int Order { get; set; }
        [MaxLength(100)]
        public string CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(200)]
        public string CreatorFullName { get; set; }
        [MaxLength(100)]
        public string ModifiedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        [MaxLength(200)]
        public string ModifierFullName { get; set; }
        [MaxLength(200)]
        public string OwnerFullName { get; set; }
        [MaxLength(100)]
        public string OwnerId { get; set; }
        [MaxLength(100)]
        public string Value { get; set; }
        public bool IsTemplate { get; set; }
    }
}
