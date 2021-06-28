using System;

namespace gRPCNet.ServerAPI.Models.Domain.Logs
{
    public class ActionLog
    {
        public int Id { get; set; }
        public string OwnerId { get; set; }
        public string UserId { get; set; }
        public string IP { get; set; }
        public int Action { get; set; }
        public string Model { get; set; }
        public string ModelId { get; set; }
        public DateTime Date { get; set; }
        public string Data { get; set; }
        public string ExceptionMessage { get; set; }
    }
}
