using Domain.Entities;

namespace Application.Interfaces
{
    public interface IGlobalEventParticipantRepository : IBaseRepository<GlobalEventParticipant>
    {
        // the participant with its event, the rolled games with their catalog games, the achievements of the picked game and the claims,
        // tracked for a change; null when the player hasn't rolled the games of the event
        public Task<GlobalEventParticipant> GetWithGamesAsync(string globalEventId, string userId);
        // false when another request added or changed the participant first, e.g. two rolls at once
        public Task<bool> TrySaveAsync();
    }
}
