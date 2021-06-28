using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace gRPCNet.ServerAPI.Models.Domain.EGMs
{
    public class GameCategory
    {
        public GameCategory()
        {
            EGMs = new List<EGM>();
        }

        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public bool HasTicket { get; set; }
        public string Value { get; set; }
        public string OwnerId { get; set; }

        public virtual IList<EGM> EGMs { get; set; }
    }
}
