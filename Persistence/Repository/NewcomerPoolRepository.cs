using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repository
{
    public class NewcomerPoolRepository : BaseRepository<NewcomerPoolGame>, INewcomerPoolRepository
    {
        private readonly IgroCehContext _context;

        public NewcomerPoolRepository(IgroCehContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> CountAsync(string eventId, string authorId = null)
        {
            return await _context.NewcomerPoolGames.CountAsync(pg => pg.EventId == eventId && (authorId == null || pg.AuthorId == authorId));
        }

        public async Task<bool> HasDroppedAsync(string poolGameId)
        {
            return await _context.EventRecords.AnyAsync(er => er.NewcomerPoolGameId == poolGameId);
        }

        public void RemoveRange(IEnumerable<NewcomerPoolGame> poolGames)
        {
            _context.NewcomerPoolGames.RemoveRange(poolGames);
        }
    }
}
