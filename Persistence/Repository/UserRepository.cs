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

        public async Task<IEnumerable<Guild>> UpdateUserGuildsAsync(long userId, IEnumerable<Guild> guilds)
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
                if(!user.Guilds.Any(g => g.Id == guild.Id))
                    user.Guilds.Add(guild);
            }

            return user.Guilds;
        }
    }
}
