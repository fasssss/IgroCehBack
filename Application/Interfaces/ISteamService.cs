using Application.DTO;

namespace Application.Interfaces
{
    // Steam Web API, methods throw SteamUnavailableException when Steam can't be reached
    public interface ISteamService
    {
        // the achievements of the game in the order of Steam, empty when the game has none
        public Task<List<SteamAchievementObject>> GetAchievementsAsync(int appId, CancellationToken ct = default);
    }
}
