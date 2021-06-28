using gRPCNet.ServerAPI.Models.Domain.Cards;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gRPCNet.ServerAPI.Models.Domain.Owners
{
    public class Owner
    {
        public Owner()
        {
            Cards = new List<Card>();
            Users = new List<UserOwner>();
        }

        [MaxLength(100)]
        public string Id { get; set; }
        [MaxLength(1000)]
        public string Name { get; set; }
        [MaxLength(10000)]
        public string Description { get; set; }
        [MaxLength(40)]
        public string PhoneNumberPrimary { get; set; }
        [MaxLength(40)]
        public string PhoneNumberSecondary { get; set; }
        [MaxLength(240)]
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime CreatedOn { get; set; }
        [MaxLength(100)]
        public string CreatorId { get; set; }
        [MaxLength(100)]
        public string CreatorName { get; set; }
        public DateTime ModifiedOn { get; set; }
        [MaxLength(100)]
        public string ModifierId { get; set; }
        [MaxLength(100)]
        public string ModifierName { get; set; }

        public virtual ICollection<Card> Cards { get; set; }
        public virtual ICollection<UserOwner> Users { get; set; }
    }
}
