﻿
namespace Domain.Entities
{
    public class User: IBaseEntity
    {
        public long Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string AvatarHash { get; set; }
        public List<Guild> Guilds { get; set; }
        public List<Event> CreatorOfEvents { get; set; }
    }
}
