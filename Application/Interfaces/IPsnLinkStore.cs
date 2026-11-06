using Application.DTO;

namespace Application.Interfaces
{
    // the links of PSN on their way and the attempts of the users; they live only in memory, after a restart of the site the user starts again
    public interface IPsnLinkStore
    {
        // false when the user is over PsnLimits.LinkAttemptsPerWindow, then PSN isn't asked
        public bool TryCountAttempt(string userId);
        public PendingPsnLinkObject GetPending(string userId);
        // a link lives PsnLimits.LinkCodeLifetimeMinutes from this call, one per user
        public void SetPending(string userId, PendingPsnLinkObject pending);
        public void RemovePending(string userId);
    }
}
