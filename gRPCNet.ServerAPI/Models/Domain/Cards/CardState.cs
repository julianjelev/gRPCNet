using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gRPCNet.ServerAPI.Models.Domain.Cards
{
    public class CardState
    {
        public CardState()
        {
            Cards = new List<Card>();
        }

        [Key]
        public string Id { get; set; }
        [MaxLength(10000)]
        public string Description { get; set; }
        public bool IsActive { get; set; }
        [MaxLength(200)]
        public string Name { get; set; }
        [MaxLength(200)]
        public string OwnerFullName { get; set; }
        [MaxLength(100)]
        public string OwnerId { get; set; }
        [MaxLength(200)]
        public string Value { get; set; }
        public virtual IList<Card> Cards { get; set; }
    }
}
