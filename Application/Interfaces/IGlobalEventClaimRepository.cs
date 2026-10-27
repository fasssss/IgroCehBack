using Domain.Entities;

namespace Application.Interfaces
{
    public interface IGlobalEventClaimRepository : IBaseRepository<GlobalEventClaim>
    {
        // the claim with its participant, tracked for a change; null when there is no such claim
        public Task<GlobalEventClaim> GetWithParticipantAsync(string claimId);
        // false when another administrator decided the claim first
        public Task<bool> TrySaveAsync();
    }
}
