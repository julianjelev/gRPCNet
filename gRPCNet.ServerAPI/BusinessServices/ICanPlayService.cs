using gRPCNet.ServerAPI.Models.Domain.Cards;
using gRPCNet.ServerAPI.Models.Domain.Common;
using gRPCNet.ServerAPI.Models.Domain.EGMs;
using gRPCNet.ServerAPI.Models.Domain.Places;
using gRPCNet.ServerAPI.Models.Dto.EGM;
using System;
using System.Collections.Generic;

namespace gRPCNet.ServerAPI.BusinessServices
{
    public interface ICanPlayService
    {
        CanPlayDto CanPlayV2(
            IServiceProvider serviceProvider,
            Concentrator concentrator,
            ControllerConfigRelay gameController,
            Card card,
            IList<ScheduleDay> scheduleDays,
            bool shouldPay,
            IEnumerable<CurrencyTemplate> currencyTemplates);
        decimal GetPricePerGame(
            IServiceProvider serviceProvider, 
            IList<ScheduleDay> egmPriceModifiers, 
            EGM game);
    }
}
