using Application.DTO;
using Application.Enum;
using Application.Interfaces;

namespace Application.Services
{
    // the catalog knows no games of PSN, so the administrator types the id of the trophy list (with its service it is the ref) and the catalog id is ignored;
    // a ref out of the format gives no game
    public class PsnAchievementCatalog : IAchievementPlatformCatalog
    {
        private readonly IPsnService _psnService;

        public PsnAchievementCatalog(IPsnService psnService)
        {
            _psnService = psnService;
        }

        public AchievementPlatform Platform => AchievementPlatform.Psn;

        public Task<PlatformTitleAchievementsObject> GetTitleAchievementsAsync(int appId, string titleRef, CancellationToken ct = default) =>
            _psnService.GetTitleTrophiesAsync(titleRef, ct);
    }
}
