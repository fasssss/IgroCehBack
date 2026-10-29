using System.Text.Json;
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
    // every request counts against the hourly quota of the key, so the answers are kept for hours
    public class XboxService : IXboxService
    {
        private const int MinSearchQueryLength = 2;
        private const int SearchResultsLimit = 10;
        // a page holds 150 achievements, a few games have several hundred
        private const int MaxAchievementPages = 8;
        private const int LowQuotaThreshold = 20;

        private static readonly TimeSpan SearchCacheDuration = TimeSpan.FromHours(6);
        // achievements of a released game rarely change
        private static readonly TimeSpan AchievementsCacheDuration = TimeSpan.FromHours(12);

        private readonly IOpenXblApi _openXblApi;
        private readonly XboxApiOptions _options;
        private readonly IMemoryCache _cache;
        private readonly ILogger<XboxService> _logger;

        public XboxService(IOpenXblApi openXblApi, IOptions<XboxApiOptions> options, IMemoryCache cache, ILogger<XboxService> logger)
        {
            _openXblApi = openXblApi;
            _options = options.Value;
            _cache = cache;
            _logger = logger;
        }

        public async Task<List<XboxTitleObject>> SearchTitlesAsync(string query, CancellationToken ct = default)
        {
            var normalizedQuery = string.Join(' ', (query ?? "").Split(' ', StringSplitOptions.RemoveEmptyEntries)).ToLowerInvariant();
            if (normalizedQuery.Length < MinSearchQueryLength)
            {
                return new List<XboxTitleObject>();
            }

            var cacheKey = $"xbox-search:{normalizedQuery}";
            if (_cache.TryGetValue(cacheKey, out List<XboxTitleObject> cachedTitles))
            {
                return cachedTitles;
            }

            RequireKey();
            var search = await SendAsync(() => _openXblApi.SearchStore(_options.Key, normalizedQuery, "titleid", "Windows.Xbox", ct), ct);
            // the editions of a game share its title id, the first one found stands for all of them
            var titles = (search.Payload?.SearchResults ?? new List<OpenXblStoreProduct>())
                .Select(product => new XboxTitleObject { TitleId = TitleIdOf(product.XboxTitleId), Name = product.Title, ImageUrl = ImageOf(product) })
                .Where(title => title.TitleId != null && !string.IsNullOrWhiteSpace(title.Name))
                .DistinctBy(title => title.TitleId)
                .Take(SearchResultsLimit)
                .ToList();
            _cache.Set(cacheKey, titles, SearchCacheDuration);

            return titles;
        }

        public async Task<XboxTitleAchievementsObject> GetTitleAchievementsAsync(string titleId, CancellationToken ct = default)
        {
            if (!IsTitleId(titleId))
            {
                return null;
            }

            var cacheKey = $"xbox-achievements:{titleId}";
            if (_cache.TryGetValue(cacheKey, out XboxTitleAchievementsObject cachedTitle))
            {
                return cachedTitle;
            }

            RequireKey();

            // the names come in the language asked for, so the list is read for both languages of the site
            var english = GetAllAchievementsAsync(titleId, "en-US", ct);
            var russian = GetAllAchievementsAsync(titleId, "ru-RU", ct);
            await Task.WhenAll(english, russian);

            XboxTitleAchievementsObject title = null;
            if (english.Result.Count > 0)
            {
                var russianById = russian.Result.DistinctBy(a => a.Id).ToDictionary(a => a.Id);
                var titleName = english.Result
                    .SelectMany(a => a.TitleAssociations ?? new List<OpenXblTitleAssociation>())
                    .FirstOrDefault(association => association.Id.ToString() == titleId)?.Name;
                title = new XboxTitleAchievementsObject
                {
                    TitleId = titleId,
                    Name = string.IsNullOrWhiteSpace(titleName) ? titleId : titleName,
                    Achievements = english.Result
                        .Where(a => !string.IsNullOrEmpty(a.Id))
                        .DistinctBy(a => a.Id)
                        .Select(achievement =>
                        {
                            var translated = russianById.GetValueOrDefault(achievement.Id);
                            return new XboxAchievementObject
                            {
                                ApiName = achievement.Id,
                                Name = achievement.Name,
                                NameRu = string.IsNullOrWhiteSpace(translated?.Name) ? achievement.Name : translated.Name,
                                Description = DescriptionOf(achievement),
                                DescriptionRu = DescriptionOf(translated) ?? DescriptionOf(achievement),
                                IconUrl = achievement.MediaAssets?.FirstOrDefault(asset => asset.Type == "Icon")?.Url ?? achievement.MediaAssets?.FirstOrDefault()?.Url,
                                Percent = achievement.Rarity?.CurrentPercentage is double percent ? Math.Round(percent, 1) : null,
                                IsHidden = achievement.IsSecret,
                            };
                        })
                        // the ids are numbers given in the order of the game, the list comes in another order
                        .OrderBy(a => long.TryParse(a.ApiName, out var number) ? number : long.MaxValue)
                        .ThenBy(a => a.ApiName, StringComparer.Ordinal)
                        .ToList(),
                };
            }

            _cache.Set(cacheKey, title, AchievementsCacheDuration);

            return title;
        }

        private async Task<List<OpenXblAchievement>> GetAllAchievementsAsync(string titleId, string language, CancellationToken ct)
        {
            var achievements = new List<OpenXblAchievement>();
            string continuationToken = null;
            for (var page = 0; page < MaxAchievementPages; page++)
            {
                var token = continuationToken;
                var content = await SendAsync(() => _openXblApi.GetTitleAchievements(_options.Key, language, titleId, token, ct), ct);
                achievements.AddRange(content.Achievements ?? new List<OpenXblAchievement>());
                continuationToken = content.PagingInfo?.ContinuationToken;
                if (string.IsNullOrEmpty(continuationToken))
                {
                    break;
                }
            }

            return achievements;
        }

        // title ids are unsigned 32-bit numbers
        private static bool IsTitleId(string titleId) => !string.IsNullOrEmpty(titleId) && titleId.Length <= 10 && uint.TryParse(titleId, out _)
            && titleId.All(char.IsAsciiDigit);

        private static string TitleIdOf(JsonElement? value) => value?.ValueKind switch
        {
            JsonValueKind.Number => value.Value.GetRawText(),
            JsonValueKind.String => IsTitleId(value.Value.GetString()) ? value.Value.GetString() : null,
            _ => null,
        };

        // the box art fits the cover of a game best
        private static string ImageOf(OpenXblStoreProduct product)
        {
            var images = product.Images ?? new List<OpenXblStoreImage>();
            var image = images.FirstOrDefault(i => i.ImageType == "BoxArt") ?? images.FirstOrDefault(i => i.ImageType == "Poster") ?? images.FirstOrDefault();
            var url = image?.Url;
            // the store gives the addresses without the scheme
            return string.IsNullOrEmpty(url) ? null : url.StartsWith("//") ? "https:" + url : url;
        }

        private static string DescriptionOf(OpenXblAchievement achievement) =>
            achievement == null ? null
            : !string.IsNullOrWhiteSpace(achievement.Description) ? achievement.Description
            : string.IsNullOrWhiteSpace(achievement.LockedDescription) ? null : achievement.LockedDescription;

        private void RequireKey()
        {
            if (string.IsNullOrWhiteSpace(_options.Key))
            {
                throw new XboxUnavailableException("There is no OpenXBL API key");
            }
        }

        private async Task<T> SendAsync<T>(Func<Task<ApiResponse<OpenXblResponse<T>>>> request, CancellationToken ct) where T : class
        {
            ApiResponse<OpenXblResponse<T>> response;
            try
            {
                response = await request();
            }
            catch (Exception e) when ((e is HttpRequestException or ApiException or TaskCanceledException) && !ct.IsCancellationRequested)
            {
                _logger.LogError(e, "OpenXBL can't be reached");
                throw new XboxUnavailableException("OpenXBL can't be reached", e);
            }

            LogQuota(response);
            if (!response.IsSuccessStatusCode || response.Content?.Content == null)
            {
                _logger.LogError(response.Error, "OpenXBL answered with {StatusCode}", (int)response.StatusCode);
                throw new XboxUnavailableException($"OpenXBL answered with {(int)response.StatusCode}", response.Error);
            }

            return response.Content.Content;
        }

        private void LogQuota(IApiResponse response)
        {
            if (response.Headers.TryGetValues("X-RateLimit-Remaining", out var values)
                && int.TryParse(values.FirstOrDefault(), out var remaining)
                && remaining <= LowQuotaThreshold)
            {
                _logger.LogWarning("OpenXBL quota is running low: {Remaining} requests left", remaining);
            }
        }
    }
}
