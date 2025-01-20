using System.Numerics;

namespace Domain.Entities
{
    public class EventRecord: IBaseEntity
    {
        public string Id { get; set; }
        public string ParticipantId { get; set; }
        public User Participant { get; set; }
        public string? ToUserId { get; set; }
        public User ToUser { get; set; }
        public string? GameId { get; set; }
        public Game Game { get; set; }
        public string EventId { get; set; }
        public Event Event { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? SucceededAt { get; set; }
        public int? Reward { get; set; }
    }
}
