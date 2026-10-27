namespace Domain.Entities
{
    // a player who rolled the games of a global event, once per event: the player picks one of the rolled games
    // and may refuse it for a new roll when a week has passed since the pick
    public class GlobalEventParticipant : IBaseEntity
    {
        public string Id { get; set; }
        public string GlobalEventId { get; set; }
        public GlobalEvent GlobalEvent { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }
        // the games of the last roll, a new roll replaces them
        public List<GlobalEventRolledGame> RolledGames { get; set; } = new List<GlobalEventRolledGame>();
        public DateTimeOffset RolledAt { get; set; }
        // one of the rolled games, empty until the player picks it
        public string? PickedGameId { get; set; }
        public GlobalEventGame? PickedGame { get; set; }
        public DateTimeOffset? PickedAt { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
