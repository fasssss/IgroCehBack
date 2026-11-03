using Application.Consts;
using Application.DTO;
using Application.Enum;
using Application.Interfaces;

namespace Application.Services
{
    // the catalog id of a game in Steam is its Steam app id, so Steam needs no game of its own and ignores the ref;
    // a game outside Steam has no achievements there, so Steam isn't asked
    public class SteamAchievementCatalog : IAchievementPlatformCatalog
    {
        private readonly ISteamService _steamService;

        public SteamAchievementCatalog(ISteamService steamService)
        {
            _steamService = steamService;
        }

        public AchievementPlatform Platform => AchievementPlatform.Steam;

        public async Task<PlatformTitleAchievementsObject> GetTitleAchievementsAsync(int appId, string titleRef, CancellationToken ct = default)
        {
            if (!CatalogIds.IsSteamAppId(appId))
            {
                return null;
            }

            var achievements = await _steamService.GetAchievementsAsync(appId, ct);
            return achievements.Count == 0 ? null : new PlatformTitleAchievementsObject { Achievements = achievements };
        }
    }
}
