using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserRepository: IBaseRepository<User>
    {
        public Task<IEnumerable<UserGuild>> UpdateUserGuildsAsync(string userId, IEnumerable<UserGuild> guilds);
        public Task<IEnumerable<UserGuild>> GetUserGuildsByEventIdAsync(string eventId);
        public Task<IEnumerable<UserGuild>> GetUserGuildsByGuildIdAsync(string guildId);
    }
}
