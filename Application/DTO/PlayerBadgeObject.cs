namespace Application.DTO
{
    // a badge a player got for passing a global event
    public class PlayerBadgeObject
    {
        // the name of a badge image of the front
        public string BadgeId { get; set; }
        public string GlobalEventId { get; set; }
        public string GlobalEventName { get; set; }
        public DateTimeOffset? PassedAt { get; set; }
    }
}
