using Domain.Entities;

namespace Application.Interfaces
{
    public interface IGlobalEventRepository : IBaseRepository<GlobalEvent>
    {
        // the event with its games, their catalog games and achievements, tracked for a change; null when there is no such event
        public Task<GlobalEvent> GetWithGamesAsync(string globalEventId);
    }
}
