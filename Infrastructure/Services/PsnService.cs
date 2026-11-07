using System.Net;
using Application.Consts;
using Application.DTO;
using Application.Exceptions;
using Application.Interfaces;
using Infrastructure.DTO;
using Infrastructure.ExternalInterfaces;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Refit;

namespace Infrastructure.Services
{
    public class PsnService : IPsnService
    {
        // a few games have several hundred trophies
        private const int MaxTrophyPages = 8;

        // trophies of a released game rarely change
        private static readonly TimeSpan TrophiesCacheDuration = TimeSpan.FromHours(12);

        private readonly IPsnApi _psnApi;
        private readonly IPsnProfileApi _profileApi;
        private readonly PsnTokenStore _tokens;
        private readonly PsnRateLimiter _limiter;
        private readonly IMemoryCache _cache;
        private readonly ILogger<PsnService> _logger;

        public PsnService(IPsnApi psnApi, IPsnProfileApi profileApi, PsnTokenStore tokens, PsnRateLimiter limiter, IMemoryCache cache, ILogger<PsnService> logger)
        {
            _psnApi = psnApi;
            _profileApi = profileApi;
            _tokens = tokens;
            _limiter = limiter;
            _cache = cache;
            _logger = logger;
        }

        public async Task<PsnProfileObject> GetProfileAsync(string onlineId, CancellationToken ct = default)
        {
            if (!PsnLimits.IsOnlineId(onlineId))
            {
                return null;
            }

            var response = await SendAsync(authorization => _profileApi.GetProfile(authorization, onlineId, ct), ct);
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            var profile = Require(response).Profile;
            if (string.IsNullOrEmpty(profile?.AccountId))
            {
                _logger.LogError("PSN answered a profile without an account");
                throw new PsnUnavailableException("PSN answered a profile without an account");
            }

            return new PsnProfileObject { AccountId = profile.AccountId, OnlineId = profile.OnlineId, AboutMe = profile.AboutMe };
        }

        public async Task<PlatformTitleAchievementsObject> GetTitleTrophiesAsync(string titleRef, CancellationToken ct = default)
        {
            if (!PsnLimits.TryParseTitleRef(titleRef, out var communicationId, out var service))
            {
                return null;
            }

            var cacheKey = TrophiesCacheKey(communicationId, service);
            if (_cache.TryGetValue(cacheKey, out PlatformTitleAchievementsObject cachedTitle))
            {
                return cachedTitle;
            }

            // the id alone doesn't tell the service, the other one answers 404
            PlatformTitleAchievementsObject title = null;
            foreach (var candidate in service != null ? new[] { service } : new[] { PsnLimits.Ps5TrophyService, PsnLimits.OlderTrophyService })
            {
                title = await GetTitleTrophiesAsync(communicationId, candidate, ct);
                if (title != null)
                {
                    _cache.Set(TrophiesCacheKey(communicationId, candidate), title, TrophiesCacheDuration);
                    break;
                }
            }

            _cache.Set(cacheKey, title, TrophiesCacheDuration);
            return title;
        }

        // the progress of a player changes all the time, so it isn't kept
        public async Task<Dictionary<string, DateTimeOffset>> GetEarnedTrophiesAsync(string accountId, string titleRef, CancellationToken ct = default)
        {
            // the site saves the ref with the service, without it there is no list to ask about
            if (!PsnLimits.IsAccountId(accountId) || !PsnLimits.TryParseTitleRef(titleRef, out var communicationId, out var service) || service == null)
            {
                return new Dictionary<string, DateTimeOffset>();
            }

            var earned = new Dictionary<string, DateTimeOffset>();
            int? offset = null;
            for (var page = 0; page < MaxTrophyPages; page++)
            {
                var pageOffset = offset;
                var response = await SendAsync(authorization => _psnApi.GetPlayerTrophies(authorization, accountId, communicationId, service, pageOffset, ct), ct);
                if (response.StatusCode == HttpStatusCode.Forbidden)
                {
                    return null;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return earned;
                }

                var trophies = Require(response);
                foreach (var trophy in trophies.Trophies ?? new List<PsnTrophy>())
                {
                    if (trophy.Earned && trophy.EarnedDateTime is DateTimeOffset earnedAt)
                    {
                        earned[trophy.TrophyId.ToString()] = earnedAt;
                    }
                }

                offset = trophies.NextOffset;
                if (offset == null)
                {
                    break;
                }
            }

            return earned;
        }

        public Task<PsnStatusObject> GetStatusAsync(CancellationToken ct = default) => _tokens.GetStatusAsync(ct);

        private static string TrophiesCacheKey(string communicationId, string service) => $"psn-trophies:{PsnLimits.FormatTitleRef(communicationId, service)}";

        // null when the service doesn't keep this list
        private async Task<PlatformTitleAchievementsObject> GetTitleTrophiesAsync(string communicationId, string service, CancellationToken ct)
        {
            var english = await GetAllTrophiesAsync(communicationId, service, "en-US", ct);
            if (english == null || english.Count == 0)
            {
                return null;
            }

            // the names come in the language asked for, so the list is read for both languages of the site
            var russian = GetAllTrophiesAsync(communicationId, service, "ru-RU", ct);
            var groups = SendAsync(authorization => _psnApi.GetTitleTrophyGroups(authorization, communicationId, service, ct), ct);
            await Task.WhenAll(russian, groups);

            var russianById = (russian.Result ?? new List<PsnTrophy>()).DistinctBy(t => t.TrophyId).ToDictionary(t => t.TrophyId);
            var titleName = Require(groups.Result).TrophyTitleName;
            return new PlatformTitleAchievementsObject
            {
                TitleRef = PsnLimits.FormatTitleRef(communicationId, service),
                TitleName = string.IsNullOrWhiteSpace(titleName) ? communicationId : titleName,
                Achievements = english
                    .DistinctBy(t => t.TrophyId)
                    .OrderBy(t => t.TrophyId)
                    .Select(trophy =>
                    {
                        var translated = russianById.GetValueOrDefault(trophy.TrophyId);
                        return new PlatformAchievementObject
                        {
                            ApiName = trophy.TrophyId.ToString(),
                            Name = trophy.TrophyName,
                            NameRu = string.IsNullOrWhiteSpace(translated?.TrophyName) ? trophy.TrophyName : translated.TrophyName,
                            Description = trophy.TrophyDetail,
                            DescriptionRu = string.IsNullOrWhiteSpace(translated?.TrophyDetail) ? trophy.TrophyDetail : translated.TrophyDetail,
                            IconUrl = trophy.TrophyIconUrl,
                            IsHidden = trophy.TrophyHidden,
                        };
                    })
                    .ToList(),
            };
        }

        // null when the service doesn't keep this list
        private async Task<List<PsnTrophy>> GetAllTrophiesAsync(string communicationId, string service, string language, CancellationToken ct)
        {
            var all = new List<PsnTrophy>();
            int? offset = null;
            for (var page = 0; page < MaxTrophyPages; page++)
            {
                var pageOffset = offset;
                var response = await SendAsync(authorization => _psnApi.GetTitleTrophies(authorization, language, communicationId, service, pageOffset, ct), ct);
                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return null;
                }

                var trophies = Require(response);
                all.AddRange(trophies.Trophies ?? new List<PsnTrophy>());
                offset = trophies.NextOffset;
                if (offset == null)
                {
                    break;
                }
            }

            return all;
        }

        // an access token refused before its time (a new NPSSO revokes the sessions of the old one) is dropped and the request repeats once
        private async Task<ApiResponse<T>> SendAsync<T>(Func<string, Task<ApiResponse<T>>> request, CancellationToken ct)
        {
            var (response, token) = await SendOnceAsync(request, ct);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _tokens.Invalidate(token);
                (response, _) = await SendOnceAsync(request, ct);
            }

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                _logger.LogWarning("PSN answered with 429, the limit of the site may be too high");
                throw new PsnUnavailableException("PSN answered with 429");
            }

            return response;
        }

        private async Task<(ApiResponse<T> Response, string Token)> SendOnceAsync<T>(Func<string, Task<ApiResponse<T>>> request, CancellationToken ct)
        {
            var token = await _tokens.GetAccessTokenAsync(ct);
            _limiter.Acquire();
            try
            {
                return (await request($"Bearer {token}"), token);
            }
            catch (Exception e) when ((e is HttpRequestException or ApiException or TaskCanceledException) && !ct.IsCancellationRequested)
            {
                _logger.LogError(e, "PSN can't be reached");
                throw new PsnUnavailableException("PSN can't be reached", e);
            }
        }

        private T Require<T>(ApiResponse<T> response) where T : class
        {
            if (!response.IsSuccessStatusCode || response.Content == null)
            {
                _logger.LogError("PSN answered with {StatusCode}", (int)response.StatusCode);
                throw new PsnUnavailableException($"PSN answered with {(int)response.StatusCode}");
            }

            return response.Content;
        }
    }
}
