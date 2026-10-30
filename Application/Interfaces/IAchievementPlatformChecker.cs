using Application.Enum;
using Domain.Entities;

namespace Application.Interfaces
{
    // a platform whose achievements prove the passing of a game of a global event; the participation asks the platform the player chose
    // and knows nothing of the platforms themselves, so a new platform is one more implementation registered next to the others;
    // the methods throw AchievementPlatformUnavailableException when the platform doesn't answer
    public interface IAchievementPlatformChecker
    {
        public AchievementPlatform Platform { get; }
        // the account of the player on the platform, null when it isn't linked
        public Task<string> GetAccountIdAsync(string userId);
        // whether an achievement of the game chosen on the platform was unlocked after the pick
        public Task<PassingCheckResult> CheckAsync(string accountId, GlobalEventGame game, DateTimeOffset pickedAt, CancellationToken ct = default);
        // the games the player has unlocked a chosen achievement of already, they don't drop in a roll;
        // a platform that can't tell it cheaply gives none
        public Task<HashSet<string>> GetPassedGameIdsAsync(string accountId, List<GlobalEventGame> games, CancellationToken ct = default);
    }
}
