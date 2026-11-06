using Application.Consts;
using Application.DTO;
using Application.Interfaces;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services
{
    // one for the whole site, the lock keeps a burst of requests of one user from getting past the limit
    public class PsnLinkStore : IPsnLinkStore
    {
        private readonly IMemoryCache _cache;
        private readonly object _attemptsLock = new();

        public PsnLinkStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public bool TryCountAttempt(string userId)
        {
            lock (_attemptsLock)
            {
                var attempts = _cache.GetOrCreate(AttemptsKey(userId), entry =>
                {
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(PsnLimits.LinkAttemptsWindowMinutes);
                    return new Attempts();
                });
                attempts.Count++;
                return attempts.Count <= PsnLimits.LinkAttemptsPerWindow;
            }
        }

        public PendingPsnLinkObject GetPending(string userId)
        {
            return _cache.Get<PendingPsnLinkObject>(PendingKey(userId));
        }

        public void SetPending(string userId, PendingPsnLinkObject pending)
        {
            _cache.Set(PendingKey(userId), pending, TimeSpan.FromMinutes(PsnLimits.LinkCodeLifetimeMinutes));
        }

        public void RemovePending(string userId)
        {
            _cache.Remove(PendingKey(userId));
        }

        private static string AttemptsKey(string userId) => $"psn-link-attempts:{userId}";

        private static string PendingKey(string userId) => $"psn-link-pending:{userId}";

        private class Attempts
        {
            public int Count { get; set; }
        }
    }
}
