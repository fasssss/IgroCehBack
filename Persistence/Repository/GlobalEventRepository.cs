using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository
{
    public class GlobalEventRepository : BaseRepository<GlobalEvent>, IGlobalEventRepository
    {
        private readonly IgroCehContext _context;

        public GlobalEventRepository(IgroCehContext context) : base(context)
        {
            _context = context;
        }

        public async Task<GlobalEvent> GetWithGamesAsync(string globalEventId)
        {
            return await _context.GlobalEvents
                .Include(ge => ge.Games)
                    .ThenInclude(geg => geg.Game)
                .Include(ge => ge.Games)
                    .ThenInclude(geg => geg.Achievements)
                .FirstOrDefaultAsync(ge => ge.Id == globalEventId);
        }
    }
}
