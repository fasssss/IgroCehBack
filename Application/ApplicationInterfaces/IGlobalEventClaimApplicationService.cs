using Application.DTO;
using Application.Enum;

namespace Application.ApplicationInterfaces
{
    // the administrators of the site check the claims that the picked games of a global event are passed
    public interface IGlobalEventClaimApplicationService
    {
        // the claims under review first, the oldest of them on top, then the decided ones; null when there is no such event
        public Task<List<GlobalEventClaimObject>> GetClaimsAsync(string globalEventId);
        public Task<DecideGlobalEventClaimResult> DecideAsync(string userId, string claimId, bool isApproved, string comment);
    }
}
