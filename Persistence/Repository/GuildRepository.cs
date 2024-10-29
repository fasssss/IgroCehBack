using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Persistence.Repository
{
    public class GuildRepository : BaseRepository<Guild>, IGuildRepository
    {
        private readonly IgroCehContext _context;

        public GuildRepository(IgroCehContext context): base(context) 
        {
            _context = context;
        }

        public async Task<ICollection<Guild>> GetGuildsByUserIdAsync(string id)
        {
            var guilds = await _context.Users
                .Where(u => u.Id == id)
                .SelectMany(ug => ug.UserGuilds)
                .Select(ug => ug.Guild)
                .ToListAsync();
            return guilds;
        }

        public async Task<ICollection<Guild>> GetFilteredByUserIdAndNameAsync(string id, string searchString)
        {
            var guilds = await _context.Users
                .Where(u => u.Id == id)
                .SelectMany(u => u.UserGuilds)
                .Where(ug => ug.Guild.Name.Contains(searchString))
                .Select(ug => ug.Guild)
                .ToListAsync();
            return guilds;
        }
    }
}
