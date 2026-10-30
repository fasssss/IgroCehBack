using System.Net;
using Application.Consts;
using Application.DTO;
using Application.Exceptions;
using Application.Interfaces;
using Infrastructure.Configurations;
using Infrastructure.DTO;
using Infrastructure.ExternalInterfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Refit;

namespace Infrastructure.Services
{
    public class VideoGamesCriticCatalogService : IGameCatalogService
    {
        private const int SearchResultsLimit = 10;
        private const int MinSearchQueryLength = 2; // the API rejects shorter queries
        private const int LowQuotaThreshold = 20;

        // names and covers barely change, scores are refreshed by the catalog every couple of hours
        private static readonly TimeSpan SearchCacheDuration = TimeSpan.FromHours(6);

        private readonly IVideoGamesCriticApi _videoGamesCriticApi;
        private readonly IMemoryCache _cache;
        private readonly ILogger<VideoGamesCriticCatalogService> _logger;
        private readonly Uri _catalogAddress;

        public VideoGamesCriticCatalogService(IVideoGamesCriticApi videoGamesCriticApi,
            IMemoryCache cache,
            ILogger<VideoGamesCriticCatalogService> logger,
            IOptions<VideoGamesCriticOptions> options)
        {
            _videoGamesCriticApi = videoGamesCriticApi;
            _cache = cache;
            _logger = logger;
            _catalogAddress = new Uri(options.Value.Address);
        }

        public async Task<List<CatalogGameObject>> SearchGamesAsync(string query, CancellationToken ct = default)
        {
            var normalizedQuery = string.Join(' ', (query ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
            if (normalizedQuery.Length < MinSearchQueryLength)
            {
                return new List<CatalogGameObject>();
            }

            var cacheKey = $"vgc-search:{normalizedQuery}";
            if (_cache.TryGetValue(cacheKey, out List<CatalogGameObject> cachedGames))
            {
                return cachedGames;
            }

            var response = await SendAsync(() => _videoGamesCriticApi.SearchGames(normalizedQuery, SearchResultsLimit, ct), ct);
            var games = (response.Content.Games ?? new List<VgcGame>()).Select(ToCatalogGame).ToList();
            _cache.Set(cacheKey, games, SearchCacheDuration);

            // a game is picked from the search results, so saving it later doesn't need one more request
            foreach (var game in games)
            {
                _cache.Set(GameCacheKey(game.VgcId), game, SearchCacheDuration);
            }

            return games;
        }

        public async Task<CatalogGameObject> GetGameAsync(int vgcId, CancellationToken ct = default)
        {
            if (_cache.TryGetValue(GameCacheKey(vgcId), out CatalogGameObject cachedGame))
            {
                return cachedGame;
            }

            var response = await SendAsync(() => _videoGamesCriticApi.GetGame(vgcId, ct), ct, allowNotFound: true);
            return response.StatusCode == HttpStatusCode.NotFound ? null : ToCatalogGame(response.Content);
        }

        private static string GameCacheKey(int vgcId) => $"vgc-game:{vgcId}";

        private async Task<ApiResponse<T>> SendAsync<T>(Func<Task<ApiResponse<T>>> request, CancellationToken ct, bool allowNotFound = false)
        {
            ApiResponse<T> response;
            try
            {
                response = await request();
            }
            catch (Exception e) when ((e is HttpRequestException or ApiException or TaskCanceledException) && !ct.IsCancellationRequested)
            {
                _logger.LogError(e, "VideoGamesCritic can't be reached");
                throw new GameCatalogUnavailableException("VideoGamesCritic can't be reached", e);
            }

            LogQuota(response);
            if (allowNotFound && response.StatusCode == HttpStatusCode.NotFound)
            {
                return response;
            }

            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                _logger.LogError(response.Error, "VideoGamesCritic answered with {StatusCode}", (int)response.StatusCode);
                throw new GameCatalogUnavailableException($"VideoGamesCritic answered with {(int)response.StatusCode}", response.Error);
            }

            return response;
        }

        private void LogQuota(IApiResponse response)
        {
            if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var values)
                && int.TryParse(values.FirstOrDefault(), out var remaining)
                && remaining <= LowQuotaThreshold)
            {
                _logger.LogWarning("VideoGamesCritic quota is running low: {Remaining} requests left", remaining);
            }
        }

        private CatalogGameObject ToCatalogGame(VgcGame game)
        {
            return new CatalogGameObject
            {
                VgcId = game.Id,
                Name = game.Name,
                ReleaseDate = game.ReleaseDate,
                CoverUrl = ToAbsoluteUrl(game.Art?.Cover),
                PageUrl = game.Links?.Page ?? game.Url,
                // search results come without store links, then the link is made from the id of a game in Steam;
                // a game outside Steam has none, the details give it null
                SteamUrl = game.Links?.Steam ?? (CatalogIds.IsSteamAppId(game.Id) ? $"https://store.steampowered.com/app/{game.Id}/" : null),
                Platforms = game.Platforms ?? new List<string>(),
                CriticsScore = Round(game.Scores?.Critics),
                PlayersScore = Round(game.Scores?.Players),
            };
        }

        // a cover that isn't on Steam comes through the image proxy of the catalog as a path on its site,
        // e.g. /api/img?u=<the original image>&w=480, a small webp the catalog caches for good;
        // the path is read as relative on purpose: on Linux a string starting with a slash is an absolute file path to Uri
        private string ToAbsoluteUrl(string url)
        {
            if (string.IsNullOrEmpty(url) || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return url;
            }

            return Uri.TryCreate(url, UriKind.Relative, out var path) ? new Uri(_catalogAddress, path).AbsoluteUri : null;
        }

        private static int? Round(double? score) => score.HasValue ? (int)Math.Round(score.Value) : null;
    }
}
