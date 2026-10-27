using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository
{
    public class GlobalEventParticipantRepository : BaseRepository<GlobalEventParticipant>, IGlobalEventParticipantRepository
    {
        private readonly IgroCehContext _context;

        public GlobalEventParticipantRepository(IgroCehContext context) : base(context)
        {
            _context = context;
        }

        public async Task<GlobalEventParticipant> GetWithGamesAsync(string globalEventId, string userId)
        {
            return await _context.GlobalEventParticipants
                .Include(p => p.GlobalEvent)
                .Include(p => p.RolledGames)
                    .ThenInclude(rg => rg.GlobalEventGame)
                        .ThenInclude(geg => geg.Game)
                .FirstOrDefaultAsync(p => p.GlobalEventId == globalEventId && p.UserId == userId);
        }

        // the unique index of the player in the event and the concurrency tokens of the roll fail the second of two requests
        public async Task<bool> TrySaveAsync()
        {
            try
            {
                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                return false;
            }
        }
    }
}
