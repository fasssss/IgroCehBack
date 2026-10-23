using Domain.Entities;
using System.Linq.Expressions;

namespace Application.Interfaces
{
    public interface IReviewRepository : IBaseRepository<Review>
    {
        // the record with its event, players, game and the review with its sections
        public Task<EventRecord> GetEventRecordWithReviewAsync(string eventRecordId);
        // records of events that aren't deleted, for the games whose review isn't written yet
        public IQueryable<EventRecord> WhereEventRecords(Expression<Func<EventRecord, bool>> expression);
    }
}
