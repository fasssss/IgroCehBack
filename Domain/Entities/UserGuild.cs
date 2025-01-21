using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class UserGuild
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public string GuildId { get; set; }
        public Guild Guild { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int Score { get; set; } = 0;
    }
}
