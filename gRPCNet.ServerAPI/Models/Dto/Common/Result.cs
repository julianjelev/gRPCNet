using System.Collections.Generic;
using System.Linq;

namespace gRPCNet.ServerAPI.Models.Dto.Common
{
    public class Result<T> : IResult<T>
    {
        public Result(bool isSuccess = false)
        {
            Messages = new List<string>();
            IsSuccess = isSuccess;
        }

        public bool IsSuccess { get; set; }
        public T Data { get; set; }
        public ICollection<string> Messages { get; set; }

        public string ShowMessages()
        {
            return string.Join(" ", this.Messages);
        }

        public bool Success()
        {
            if (IsSuccess)
            {
                return IsSuccess;
            }

            return !Messages.Any();
        }
    }
}
