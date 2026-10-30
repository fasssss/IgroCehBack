using Application.Enum;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;

namespace Application.Services
{
    // the check every platform makes the same way: one of the achievements of the game chosen on the platform unlocked after the pick
    // passes it, the ones unlocked before don't count; a platform gives the unlocks of the player and the precision of their time
    public abstract class AchievementPlatformChecker : IAchievementPlatformChecker
    {
        public abstract AchievementPlatform Platform { get; }

        public abstract Task<string> GetAccountIdAsync(string userId);

        public abstract Task<HashSet<string>> GetPassedGameIdsAsync(string accountId, List<GlobalEventGame> games, CancellationToken ct = default);

        public async Task<PassingCheckResult> CheckAsync(string accountId, GlobalEventGame game, DateTimeOffset pickedAt, CancellationToken ct = default)
        {
            var unlocked = await GetUnlockedAchievementsAsync(accountId, game, ct);
            if (unlocked == null)
            {
                return PassingCheckResult.ProfileHidden;
            }

            var pickedAtOnPlatform = ToPlatformPrecision(pickedAt);
            var unlockTimes = AchievementsOf(game)
                .Where(a => unlocked.ContainsKey(a.ApiName))
                .Select(a => unlocked[a.ApiName])
                .ToList();
            return unlockTimes.Any(unlockedAt => unlockedAt >= pickedAtOnPlatform) ? PassingCheckResult.Passed
                : unlockTimes.Count > 0 ? PassingCheckResult.UnlockedBeforePick
                : PassingCheckResult.NotPassed;
        }

        // the unlocked achievements of the player in the game by their ids on the platform with the time of the unlock,
        // null when the platform hides them
        protected abstract Task<Dictionary<string, DateTimeOffset>> GetUnlockedAchievementsAsync(string accountId, GlobalEventGame game, CancellationToken ct);

        // the pick as precise as the platform keeps the time of an unlock
        protected virtual DateTimeOffset ToPlatformPrecision(DateTimeOffset pickedAt) => pickedAt;

        // the platforms have ids of their own, one knows nothing about the achievements of another
        protected List<GlobalEventAchievement> AchievementsOf(GlobalEventGame game) =>
            game.Achievements.Where(a => a.Platform == (GlobalEventAchievementPlatform)Platform).ToList();
    }
}
