
namespace Domain.Entities
{
    public class User: IBaseEntity
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AvatarUrl { get; set; }
        public List<Guild> Guilds { get; set; }
        public List<Event> CreatorOfEvents { get; set; }
    }
}
