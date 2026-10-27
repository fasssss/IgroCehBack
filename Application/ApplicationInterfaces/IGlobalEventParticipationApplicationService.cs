using Application.DTO;
using Application.Enum;

namespace Application.ApplicationInterfaces
{
    // a player rolls three games of a global event, picks one of them and may refuse it for a new roll a week after the pick
    public interface IGlobalEventParticipationApplicationService
    {
        // the participation is null when the player hasn't rolled yet
        public Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> GetAsync(string userId, string globalEventId);
        public Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> RollAsync(string userId, string globalEventId,
            CancellationToken ct = default);
        public Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> PickAsync(string userId, string globalEventId, int appId);
        public Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> RefuseAsync(string userId, string globalEventId,
            CancellationToken ct = default);
    }
}
