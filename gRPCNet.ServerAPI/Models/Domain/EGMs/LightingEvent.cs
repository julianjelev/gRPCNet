using System.Collections.Generic;

namespace gRPCNet.ServerAPI.Models.Domain.EGMs
{
    public class LightingEvent
    {
        public LightingEvent()
        {
            EGMs = new List<EGM>();
        }
        public int Id { get; set; }
        public string Name { get; set; }
        public string OwnerId { get; set; }
        public bool IsActive { get; set; }
        public int NotLoggedId { get; set; }
        public int LoggedId { get; set; }
        public int WaitingId { get; set; }
        public int PlayId { get; set; }
        public int NoPlayId { get; set; }
        public int TryAgainId { get; set; }
        public int CountersId { get; set; }
        public int TimeCardId { get; set; }
        public int DisabledId { get; set; }
        public int InhibitId { get; set; }        

        public virtual IList<EGM> EGMs { get; set; }

        public virtual LightingProfile NotLogged { get; set; }
        public virtual LightingProfile Logged { get; set; }
        public virtual LightingProfile Waiting { get; set; }
        public virtual LightingProfile Play { get; set; }
        public virtual LightingProfile NoPlay { get; set; }
        public virtual LightingProfile TryAgain { get; set; }
        public virtual LightingProfile Counters { get; set; }
        public virtual LightingProfile TimeCard { get; set; }
        public virtual LightingProfile Disabled { get; set; }
        public virtual LightingProfile Inhibit { get; set; }
    }
}
