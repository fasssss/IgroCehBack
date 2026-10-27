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
using System.Net;

namespace Infrastructure.Services
{
    public class SteamService : ISteamService
    {
        // achievements of a released game rarely change, the shares of the players move slowly
        private static readonly TimeSpan AchievementsCacheDuration = TimeSpan.FromHours(12);

        private readonly ISteamWebApi _steamWebApi;
        private readonly SteamApiOptions _options;
        private readonly IMemoryCache _cache;
        private readonly ILogger<SteamService> _logger;

        public SteamService(ISteamWebApi steamWebApi, IOptions<SteamApiOptions> options, IMemoryCache cache, ILogger<SteamService> logger)
        {
            _steamWebApi = steamWebApi;
            _options = options.Value;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<SteamAchievementObject>> GetAchievementsAsync(int appId, CancellationToken ct = default)
        {
            var cacheKey = $"steam-achievements:{appId}";
            if (_cache.TryGetValue(cacheKey, out List<SteamAchievementObject> cachedAchievements))
            {
                return cachedAchievements;
            }

            RequireKey();

            // the names come in the language asked for, so the schema is read for both languages of the site
            var english = SendAsync(() => _steamWebApi.GetSchemaForGame(_options.Key, appId, "english", ct), ct);
            var russian = SendAsync(() => _steamWebApi.GetSchemaForGame(_options.Key, appId, "russian", ct), ct);
            var percentages = GetPercentagesAsync(appId, ct);
            await Task.WhenAll(english, russian, percentages);

            var russianByName = Achievements(russian.Result).ToDictionary(a => a.Name);
            var achievements = Achievements(english.Result)
                .Select(achievement =>
                {
                    var translated = russianByName.GetValueOrDefault(achievement.Name);
                    return new SteamAchievementObject
                    {
                        ApiName = achievement.Name,
                        Name = achievement.DisplayName,
                        NameRu = translated?.DisplayName ?? achievement.DisplayName,
                        Description = achievement.Description,
                        DescriptionRu = translated?.Description ?? achievement.Description,
                        IconUrl = achievement.Icon,
                        Percent = percentages.Result.TryGetValue(achievement.Name, out var percent) ? Math.Round(percent, 1) : null,
                        IsHidden = achievement.Hidden != 0,
                    };
                })
                .ToList();
            _cache.Set(cacheKey, achievements, AchievementsCacheDuration);

            return achievements;
        }

        public async Task<bool> IsGameDataPublicAsync(string steamId, CancellationToken ct = default)
        {
            return await GetOwnedAppIdsAsync(steamId, ct) != null;
        }

        public async Task<HashSet<int>> GetOwnedAppIdsAsync(string steamId, CancellationToken ct = default)
        {
            RequireKey();

            // an open profile without games still has the count, hidden game details give an empty response
            var ownedGames = await SendAsync(() => _steamWebApi.GetOwnedGames(_options.Key, steamId, 1, ct), ct);
            return ownedGames.Response?.GameCount == null
                ? null
                : (ownedGames.Response.Games ?? new List<SteamOwnedGame>()).Select(g => g.AppId).ToHashSet();
        }

        public async Task<Dictionary<string, DateTimeOffset>> GetUnlockedAchievementsAsync(string steamId, int appId, CancellationToken ct = default)
        {
            RequireKey();

            var response = await RequestAsync(() => _steamWebApi.GetPlayerAchievements(_options.Key, steamId, appId, ct), ct);
            if (response.StatusCode == HttpStatusCode.Forbidden)
            {
                return null;
            }

            if (response.StatusCode == HttpStatusCode.BadRequest)
            {
                return new Dictionary<string, DateTimeOffset>();
            }

            return (Successful(response).PlayerStats?.Achievements ?? new List<SteamPlayerAchievement>())
                .Where(a => a.Achieved != 0)
                .GroupBy(a => a.ApiName)
                .ToDictionary(group => group.Key, group => DateTimeOffset.FromUnixTimeSeconds(group.First().UnlockTime));
        }

        private void RequireKey()
        {
            if (string.IsNullOrWhiteSpace(_options.Key))
            {
                throw new SteamUnavailableException("There is no Steam Web API key");
            }
        }

        private static List<SteamSchemaAchievement> Achievements(SteamSchemaResponse schema) =>
            schema.Game?.AvailableGameStats?.Achievements ?? new List<SteamSchemaAchievement>();

        // the shares only help to choose, a game without them still gives its achievements
        private async Task<Dictionary<string, double>> GetPercentagesAsync(int appId, CancellationToken ct)
        {
            try
            {
                var response = await _steamWebApi.GetGlobalAchievementPercentages(appId, ct);
                return (response.IsSuccessStatusCode ? response.Content?.AchievementPercentages?.Achievements : null)?
                    .GroupBy(a => a.Name)
                    .ToDictionary(group => group.Key, group => group.First().Percent)
                    ?? new Dictionary<string, double>();
            }
            catch (Exception e) when ((e is HttpRequestException or ApiException or TaskCanceledException) && !ct.IsCancellationRequested)
            {
                _logger.LogWarning(e, "Steam didn't give the shares of the achievements of {AppId}", appId);
                return new Dictionary<string, double>();
            }
        }

        private async Task<T> SendAsync<T>(Func<Task<ApiResponse<T>>> request, CancellationToken ct)
        {
            return Successful(await RequestAsync(request, ct));
        }

        private async Task<ApiResponse<T>> RequestAsync<T>(Func<Task<ApiResponse<T>>> request, CancellationToken ct)
        {
            try
            {
                return await request();
            }
            catch (Exception e) when ((e is HttpRequestException or ApiException or TaskCanceledException) && !ct.IsCancellationRequested)
            {
                _logger.LogError(e, "Steam can't be reached");
                throw new SteamUnavailableException("Steam can't be reached", e);
            }
        }

        private T Successful<T>(ApiResponse<T> response)
        {
            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                _logger.LogError(response.Error, "Steam answered with {StatusCode}", (int)response.StatusCode);
                throw new SteamUnavailableException($"Steam answered with {(int)response.StatusCode}", response.Error);
            }

            return response.Content;
        }
    }
}
