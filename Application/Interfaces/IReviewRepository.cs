using Domain.Entities;

namespace Application.Interfaces
{
    public interface IReviewRepository : IBaseRepository<Review>
    {
        // the record with its event, players, game and the review with its sections
        public Task<EventRecord> GetEventRecordWithReviewAsync(string eventRecordId);
    }
}
