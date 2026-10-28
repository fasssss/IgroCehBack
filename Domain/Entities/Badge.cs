namespace Domain.Entities
{
    // the reward a player gets for passing a global event: an image the administrator uploads (SVG, PNG or WebP);
    // the image never changes, a new picture is a new badge, so a badge given to players stays as they got it
    public class Badge : IBaseEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
        // empty for the badges that came with the site
        public string? CreatedById { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
