using System;

namespace gRPCNet.ServerAPI.Models.Domain.Logs
{
    public class TransactionLog
    {
        public int Id { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CardId { get; set; }
        public string CardType { get; set; }
        public string SenderIP { get; set; }
        public bool IsSuccess { get; set; }
        public string Service { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountAsBonus { get; set; }
        public string AdditionalInfo { get; set; }
        public string PlaceId { get; set; }
        public string OwnerId { get; set; }
        public string GameId { get; set; }
    }
}
