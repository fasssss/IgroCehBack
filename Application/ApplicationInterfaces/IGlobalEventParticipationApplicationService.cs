using Application.DTO;
using Application.Enum;

namespace Application.ApplicationInterfaces
{
    // a player rolls three games of a global event, picks one of them, may refuse it for a new roll a week after the pick
    // and proves its passing by Steam or by a claim an administrator checks
    public interface IGlobalEventParticipationApplicationService
    {
        // the participation is null when the player hasn't rolled yet
        public Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> GetAsync(string userId, string globalEventId);
        public Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> RollAsync(string userId, string globalEventId,
            CancellationToken ct = default);
        public Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> PickAsync(string userId, string globalEventId, int appId);
        public Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> RefuseAsync(string userId, string globalEventId,
            CancellationToken ct = default);
        // throws AchievementPlatformUnavailableException when the platform doesn't answer
        public Task<(ParticipateGlobalEventResult Result, PassingCheckResult Check, GlobalEventParticipationObject Participation)> CheckPassingAsync(
            string userId, string globalEventId, AchievementPlatform platform, CancellationToken ct = default);
        public Task<(ParticipateGlobalEventResult Result, GlobalEventParticipationObject Participation)> SubmitClaimAsync(string userId, string globalEventId,
            ClaimPlatform platform, string profile);
    }
}
