using Infrastructure.DTO;
using Refit;

namespace Infrastructure.ExternalInterfaces
{
    // https://partner.steamgames.com/doc/webapi/ISteamUserStats
    public interface ISteamWebApi
    {
        // the achievements of the game with names and descriptions in the given language ("english", "russian")
        [Get("/ISteamUserStats/GetSchemaForGame/v2/")]
        public Task<ApiResponse<SteamSchemaResponse>> GetSchemaForGame(string key, int appid, string l, CancellationToken ct);

        // the share of the players who unlocked every achievement, needs no key
        [Get("/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v2/")]
        public Task<ApiResponse<SteamPercentagesResponse>> GetGlobalAchievementPercentages(int gameid, CancellationToken ct);

        // the games of a player, https://partner.steamgames.com/doc/webapi/IPlayerService
        [Get("/IPlayerService/GetOwnedGames/v1/")]
        public Task<ApiResponse<SteamOwnedGamesResponse>> GetOwnedGames(string key, string steamid, CancellationToken ct);
    }
}
