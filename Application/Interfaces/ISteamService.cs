using Application.DTO;

namespace Application.Interfaces
{
    // Steam Web API, methods throw SteamUnavailableException when Steam can't be reached
    public interface ISteamService
    {
        // the achievements of the game in the order of Steam, empty when the game has none
        public Task<List<SteamAchievementObject>> GetAchievementsAsync(int appId, CancellationToken ct = default);
        // Steam gives the achievements of a player only when the game details of the profile are public
        public Task<bool> IsGameDataPublicAsync(string steamId, CancellationToken ct = default);
        // the Steam app ids of the games of a player, the played free ones too; null when the game details are hidden
        public Task<HashSet<int>> GetOwnedAppIdsAsync(string steamId, CancellationToken ct = default);
        // the unlocked achievements of a player in a game by their API names with the time of the unlock, empty when the player has no stats
        // in the game (e.g. doesn't own it); null when the game details are hidden
        public Task<Dictionary<string, DateTimeOffset>> GetUnlockedAchievementsAsync(string steamId, int appId, CancellationToken ct = default);
    }
}
