using Application.DTO;
using Application.Enum;
using Application.Interfaces;

namespace Application.Services
{
    // the catalog knows no Xbox title ids, so the administrator finds the game in Xbox (its title id is the ref) and the catalog id is ignored;
    // a ref that isn't a title id gives no game
    public class XboxAchievementCatalog : IAchievementPlatformCatalog
    {
        private readonly IXboxService _xboxService;

        public XboxAchievementCatalog(IXboxService xboxService)
        {
            _xboxService = xboxService;
        }

        public AchievementPlatform Platform => AchievementPlatform.Xbox;

        public async Task<PlatformTitleAchievementsObject> GetTitleAchievementsAsync(int appId, string titleRef, CancellationToken ct = default)
        {
            var title = await _xboxService.GetTitleAchievementsAsync(titleRef, ct);
            return title == null || title.Achievements.Count == 0
                ? null
                : new PlatformTitleAchievementsObject { TitleRef = title.TitleId, TitleName = title.Name, Achievements = title.Achievements };
        }
    }
}
