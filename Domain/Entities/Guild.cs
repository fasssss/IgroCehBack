
namespace Domain.Entities
{
    public class Guild: IBaseEntity
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string? AvatarUrl { get; set; }
        public List<User> Users { get; set; }
        public List<Event> Events { get; set; }
    }
}
