using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository
{
    public class UserRepository: BaseRepository<User>, IUserRepository
    {
        private IgroCehContext _context;

        public UserRepository(IgroCehContext context): base(context) 
        {
            _context = context;
        }

        public async Task<IEnumerable<UserGuild>> UpdateUserGuildsAsync(string userId, IEnumerable<UserGuild> userGuilds)
        {
            var existingUserGuilds = await _context.UserGuilds
                .Where(u => u.UserId == userId).ToListAsync();

            var userGuildsToRemove = existingUserGuilds.Select(ug => ug.GuildId).Except(userGuilds.Select(ug => ug.GuildId));
            existingUserGuilds.RemoveAll(ug => userGuildsToRemove.Any(ugtrId => ugtrId == ug.Guild.Id));

            foreach (var userGuild in userGuilds)
            {
                var existingUserGuild = existingUserGuilds.Find(ug => ug.GuildId == userGuild.GuildId);
                if (existingUserGuild == null)
                {
                    _context.UserGuilds.Add(userGuild);
                }
                else
                {
                    existingUserGuild.IsAdmin = userGuild.IsAdmin;
                }
            }

            return existingUserGuilds;
        }
    }
}
