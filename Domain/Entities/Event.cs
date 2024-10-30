using System.Numerics;

namespace Domain.Entities
{
    public class Event: IBaseEntity
    {
        public string Id {  get; set; }
        public string Name { get; set; }
        public string CreatorId { get; set; }
        public User Creator { get; set; }
        public DateTimeOffset? StartDate { get; set; }
        public DateTimeOffset? EndDate { get; set;}
        public List<EventRecord> EventRecords { get; set; }
        public string GuildId { get; set; }
        public Guild Guild { get; set; }
        public Enums.EventStatusId StatusId { get; set; }
        public EventStatus Status { get; set; }
    }
}
