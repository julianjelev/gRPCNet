using gRPCNet.ServerAPI.Models.Domain.Cards;
using gRPCNet.ServerAPI.Models.Domain.Owners;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace gRPCNet.ServerAPI.Models.Domain.Users
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class User : IdentityUser
    {
        public User()
        {
            Owners = new List<UserOwner>();
            Places = new List<UserPlace>();
        }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public virtual ICollection<UserOwner> Owners { get; set; }
        public virtual IList<Card> Cards { get; set; }
        public virtual IList<UserPlace> Places { get; set; }

        public string FullName()
        {
            return $"{this.FirstName} {this.LastName}";
        }
    }
}
