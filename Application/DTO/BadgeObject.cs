namespace Application.DTO
{
    // a badge the administrator picks for a global event, with the events that give it already; the image comes by its own request
    public class BadgeObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
        // the latest first
        public List<GlobalEventBadgeUseObject> Uses { get; set; }
    }
}
