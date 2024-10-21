using System.Numerics;

namespace Domain.Entities
{
    public class Event: IBaseEntity
    {
        public long Id {  get; set; }
        public long CreatorId { get; set; }
        public User Creator { get; set; }
        public DateTimeOffset StartDate { get; set; }
        public DateTimeOffset EndDate { get; set;}
        public List<EventRecord> EventRecords { get; set; }
        public long GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}
