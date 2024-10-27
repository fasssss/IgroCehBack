using System.Numerics;

namespace Domain.Entities
{
    public class EventRecord: IBaseEntity
    {
        public string Id { get; set; }
        public string FromUserId { get; set; }
        public User FromUser { get; set; }
        public string ToUserId { get; set; }
        public User ToUser { get; set; }
        public string GameId { get; set; }
        public Game Game { get; set; }
        public string EventId { get; set; }
        public Event Event { get; set; }
    }
}
