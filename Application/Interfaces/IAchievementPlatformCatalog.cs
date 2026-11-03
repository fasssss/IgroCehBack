using Application.DTO;
using Application.Enum;

namespace Application.Interfaces
{
    // a platform the administrator chooses the achievements of a game of a global event on; the constructor knows nothing of the platforms
    // themselves, so a new platform is one more implementation registered next to the others and next to its IAchievementPlatformChecker;
    // the methods throw AchievementPlatformUnavailableException when the platform doesn't answer
    public interface IAchievementPlatformCatalog
    {
        public AchievementPlatform Platform { get; }
        // the achievements of the catalog game on the platform, the game there given by its titleRef on a platform where it has an id of its own;
        // null when the platform has no such game, the ref isn't in its format or the game has no achievements there
        public Task<PlatformTitleAchievementsObject> GetTitleAchievementsAsync(int appId, string titleRef, CancellationToken ct = default);
    }
}
