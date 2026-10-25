using Domain.Entities;

namespace Application.Interfaces
{
    public interface INewcomerPoolRepository : IBaseRepository<NewcomerPoolGame>
    {
        // the games of the event, or only those of one author
        public Task<int> CountAsync(string eventId, string authorId = null);
        // a game that has dropped to a newcomer can't be replaced or removed
        public Task<bool> HasDroppedAsync(string poolGameId);
        public void RemoveRange(IEnumerable<NewcomerPoolGame> poolGames);
    }
}
