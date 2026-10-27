namespace Domain.Entities
{
    // an event of the whole site made by its administrator: players roll its games and prove the passing by an achievement or by a claim checked by hand;
    // one event is active at a time, the administrator finishes it by hand
    public class GlobalEvent : IBaseEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        // the badge a player gets for passing the event: the name of an image of the front (src/assets/Achievements) without the extension;
        // empty for the events made before the badges
        public string? BadgeId { get; set; }
        public string CreatorId { get; set; }
        public User Creator { get; set; }
        public List<GlobalEventGame> Games { get; set; } = new List<GlobalEventGame>();
        // the players who rolled its games; once there is one, the games and the achievements can't change
        public List<GlobalEventParticipant> Participants { get; set; } = new List<GlobalEventParticipant>();
        public DateTimeOffset? CreatedAt { get; set; }
        public DateTimeOffset? FinishedAt { get; set; }
        public string? FinishedById { get; set; }
        // soft delete, like the events of guilds: the query filter of the context hides it
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedById { get; set; }
    }
}
