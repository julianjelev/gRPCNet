using System;

namespace gRPCNet.ServerAPI.Models.Abstractions
{
    public abstract class BaseModel : IOwnerable, IDateable
    {
        public string OwnerId { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string ModifiedBy { get; set; }
    }
}
