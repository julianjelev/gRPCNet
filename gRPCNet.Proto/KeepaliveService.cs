
using ProtoBuf;
using ProtoBuf.Grpc;
using ProtoBuf.Grpc.Configuration;
using System;
using System.Collections.Generic;

namespace gRPCNet.Proto
{
    [Service]
    public interface IKeepaliveService
    {
        IAsyncEnumerable<KeepaliveResult> SubscribeAsync(CallContext context = default);
    }

    [ProtoContract]
    public class KeepaliveResult
    {
        [ProtoMember(1, DataFormat = DataFormat.WellKnown)]
        public DateTime Time { get; set; }
        [ProtoMember(2)]
        public string Client { get; set; }
    }
}