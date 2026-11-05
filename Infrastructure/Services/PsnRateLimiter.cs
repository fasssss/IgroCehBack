using System.Threading.RateLimiting;
using Application.Exceptions;
using Infrastructure.Configurations;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services
{
    // one for the whole site: every real request to PSN, the sign-in and the refresh too, takes a permit; answers from the cache don't.
    // Over the limit the request fails at once, so PSN never sees more than the limit
    public class PsnRateLimiter : IDisposable
    {
        private readonly SlidingWindowRateLimiter _limiter;
        private readonly ILogger<PsnRateLimiter> _logger;

        public PsnRateLimiter(IOptions<PsnApiOptions> options, ILogger<PsnRateLimiter> logger)
        {
            var minutes = Math.Max(1, options.Value.WindowMinutes);
            _limiter = new SlidingWindowRateLimiter(new SlidingWindowRateLimiterOptions
            {
                PermitLimit = Math.Max(1, options.Value.RequestsPerWindow),
                Window = TimeSpan.FromMinutes(minutes),
                SegmentsPerWindow = minutes,
                QueueLimit = 0,
            });
            _logger = logger;
        }

        public void Acquire()
        {
            using var lease = _limiter.AttemptAcquire();
            if (!lease.IsAcquired)
            {
                _logger.LogWarning("The requests to PSN ran over the limit of the site");
                throw new PsnUnavailableException("The requests to PSN ran over the limit of the site");
            }
        }

        public void Dispose() => _limiter.Dispose();
    }
}
