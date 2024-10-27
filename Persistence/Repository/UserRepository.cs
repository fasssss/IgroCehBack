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

        public async Task<IEnumerable<Guild>> UpdateUserGuildsAsync(string userId, IEnumerable<Guild> guilds)
        {
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Include(x => x.Guilds)
                .FirstOrDefaultAsync();
            if(user == null)
            {
                return Enumerable.Empty<Guild>();
            }

            var guildsToRemove = user.Guilds.Select(ug => ug.Id).Except(guilds.Select(g => g.Id));
            user.Guilds.RemoveAll(g => guildsToRemove.Any(gtrId => gtrId == g.Id));

            foreach (var guild in guilds)
            {
                var existingGuild = user.Guilds.Find(g => g.Id == guild.Id);
                if (existingGuild == null)
                {
                    user.Guilds.Add(guild);
                }
                else
                {
                    existingGuild.Name = guild.Name;
                    existingGuild.AvatarUrl = guild.AvatarUrl;
                }
            }

            return user.Guilds;
        }
    }
}
