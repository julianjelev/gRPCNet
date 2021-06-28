namespace gRPCNet.ServerAPI.Models.Dto.EGM
{
    public class CanPlayDto
    {
        public bool CanPlay { get; set; }
        public decimal PricePerGame { get; set; }
        public string GameCenterName { get; set; }
        public string GameName { get; set; }
        public string CurrencyInfo { get; set; }
        public string Message { get; set; }
        public decimal RemainingBalance { get; set; }
        public decimal PaidFromPrimaryBalance { get; set; }
        public decimal PaidFromBonusBalance { get; set; }
        public bool HasBonusAfterPlay { get; set; }
        public decimal BonusAfterPlay { get; set; }
        public string GameId { get; set; }
        public bool HasError { get; set; }
        public bool IsFreePeriod { get; set; }
        public string CardId { get; set; }
        public string CardNumber { get; set; }
        public string CardType { get; set; }
    }
}
