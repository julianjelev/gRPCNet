using System.Collections.Generic;

namespace gRPCNet.ServerAPI.Models.Dto.Common
{
    public interface IResult<T>
    {
        bool IsSuccess { get; set; }
        T Data { get; set; }
        ICollection<string> Messages { get; set; }
        string ShowMessages();
    }
}
