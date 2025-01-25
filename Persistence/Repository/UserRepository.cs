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

            var userGuildsToRemove = existingUserGuilds.Where(ug => !ug.IsDeleted).Select(ug => ug.GuildId).Except(userGuilds.Select(ug => ug.GuildId));
            foreach(var existingUserGuild in existingUserGuilds)
            {
                if(userGuildsToRemove.Any(ugtrId => ugtrId == existingUserGuild.GuildId))
                {
                    existingUserGuild.IsDeleted = true;
                }
            }

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
                    existingUserGuild.IsDeleted = false;
                }
            }

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

        public async Task<IEnumerable<UserGuild>> GetUserGuildsByEventIdAsync(string eventId)
        {
            var guildId = (await _context.Events.FirstOrDefaultAsync(x => x.Id == eventId))?.GuildId;
            if (guildId != null)
            {
                var userGuilds = await _context.UserGuilds.Where(x => x.GuildId == guildId).ToListAsync();
                return userGuilds;
            }

            return new List<UserGuild>();
        }
    }
}
