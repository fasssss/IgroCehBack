
namespace Domain.Entities
{
    public class Guild: IBaseEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? AvatarUrl { get; set; }
        public List<UserGuild> UserGuilds { get; set; }
        public List<Event> Events { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
