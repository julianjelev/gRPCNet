using Grpc.Core;
using gRPCNet.Proto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using ProtoBuf.Grpc;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace gRPCNet.ServerAPI.gRPCServices
{
    [Authorize]
    public class KeepaliveService : IKeepaliveService
    {
        public IAsyncEnumerable<KeepaliveResult> SubscribeAsync(CallContext context = default) => SubscribeAsyncImpl(context.ServerCallContext.GetHttpContext(), context.CancellationToken);

        private async IAsyncEnumerable<KeepaliveResult> SubscribeAsyncImpl(HttpContext context, [EnumeratorCancellation] CancellationToken cancel) 
        {
            while (!cancel.IsCancellationRequested) 
            {
                try 
                {
                    await Task.Delay(TimeSpan.FromSeconds(10), cancel);
                }
                catch 
                {
                    break;
                }
                yield return new KeepaliveResult { Time = DateTime.UtcNow, Client = context.User.FindFirstValue(ClaimTypes.NameIdentifier) };
            }
        }
    }
}
