using System;

namespace gRPCNet.ServerAPI.Models.Abstractions
{
    public interface IDateable
    {
        DateTime CreatedOn { get; set; }
        string CreatedBy { get; set; }
        DateTime ModifiedOn { get; set; }
        string ModifiedBy { get; set; }
    }
}
