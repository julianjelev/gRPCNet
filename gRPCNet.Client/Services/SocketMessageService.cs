using System;
using System.Linq;
using System.Threading;

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
            Thread.Sleep(1000);//work
            //return message;
            return message.Reverse().ToArray();
        }
    }
}
