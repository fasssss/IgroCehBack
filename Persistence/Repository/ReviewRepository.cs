using Application.Interfaces;
using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using System.Linq.Expressions;

namespace Persistence.Repository
{
    public class ReviewRepository : BaseRepository<Review>, IReviewRepository
    {
        private readonly IgroCehContext _context;

        public ReviewRepository(IgroCehContext context) : base(context)
        {
            _context = context;
        }

        public async Task<EventRecord> GetEventRecordWithReviewAsync(string eventRecordId)
        {
            return await _context.EventRecords
                .Include(er => er.Event)
                .Include(er => er.Game)
                .Include(er => er.Participant)
                .Include(er => er.ToUser)
                .Include(er => er.Review)
                    .ThenInclude(r => r.Sections)
                .FirstOrDefaultAsync(er => er.Id == eventRecordId);
        }

        public IQueryable<EventRecord> WhereEventRecords(Expression<Func<EventRecord, bool>> expression)
        {
            return _context.EventRecords.Where(expression);
        }
    }
}
