using System;
using System.Linq;

namespace gRPCNet.Client.Services
{
    public interface ISocketMessageService
    {
        byte[] ProccessRequest(byte[] message);
    }

    public class SocketMessageService : ISocketMessageService
    {
        public SocketMessageService()
        {

        }

        public byte[] ProccessRequest(byte[] message)
        {
            return message.Reverse().ToArray();
        }
    }
}
