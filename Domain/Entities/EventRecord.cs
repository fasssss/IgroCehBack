using System.Numerics;

namespace Domain.Entities
{
    public class EventRecord: IBaseEntity
    {
        public long Id { get; set; }
        public long FromUserId { get; set; }
        public User FromUser { get; set; }
        public long ToUserId { get; set; }
        public User ToUser { get; set; }
        public long GameId { get; set; }
        public Game Game { get; set; }
        public long EventId { get; set; }
        public Event Event { get; set; }
    }
}
