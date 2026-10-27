namespace Domain.Entities
{
    // a game of a global event that dropped to its participant in the last roll
    public class GlobalEventRolledGame : IBaseEntity
    {
        public string Id { get; set; }
        public string ParticipantId { get; set; }
        public GlobalEventParticipant Participant { get; set; }
        public string GlobalEventGameId { get; set; }
        public GlobalEventGame GlobalEventGame { get; set; }
        // the order the games dropped in
        public int Order { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
