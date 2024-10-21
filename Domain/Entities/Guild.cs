
namespace Domain.Entities
{
    public class Guild: IBaseEntity
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string? AvatarHash { get; set; }
        public List<User> Users { get; set; }
        public List<Event> Events { get; set; }
    }
}
