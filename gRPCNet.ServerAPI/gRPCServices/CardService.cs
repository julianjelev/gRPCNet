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
    public class CardService : ICardService
    {
        public ValueTask<CanPlayResponse> CanPlayAsync(CanPlayRequest value, CallContext context = default)
        {
            CanPlayResponse response = new CanPlayResponse();
            if (!context.CancellationToken.IsCancellationRequested)
            {                
                response.ConcentratorId = value.ConcentratorId;
                response.ControllerId = value.ControllerId;
                response.CardType = value.CardType;
                response.CardId = value.CardId;
                response.Permission = true;
            }
            return new ValueTask<CanPlayResponse>(response); 
        }

        public ValueTask<ServicePriceResponse> ServicePriceAsync(ServicePriceRequest value, CallContext context = default)
        {
            return new ValueTask<ServicePriceResponse>(new ServicePriceResponse());
        }

    }
}
