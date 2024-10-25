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

        public async Task<ICollection<Guild>> GetGuildsByUserIdAsync(long id)
        {
            var guilds = (await _context.Users.FindAsync(id))?.Guilds ?? new List<Guild>();
            return guilds;
        }

        public async Task<ICollection<Guild>> GetFilteredByUserIdAndNameAsync(long id, string searchString)
        {
            var guilds = await _context.Users
                .Where(u => u.Id == id)
                .SelectMany(u => u.Guilds)
                .Where(g => g.Name.Contains(searchString))
                .ToListAsync();
            return guilds;
        }
    }
}
