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

        // the games of a player, https://partner.steamgames.com/doc/webapi/IPlayerService; 1 adds the free games the player has played
        [Get("/IPlayerService/GetOwnedGames/v1/")]
        public Task<ApiResponse<SteamOwnedGamesResponse>> GetOwnedGames(string key, string steamid, int include_played_free_games, CancellationToken ct);

        // the achievements of a player in a game; 403 for hidden game details, 400 for a game without stats of the player, e.g. one they don't own
        [Get("/ISteamUserStats/GetPlayerAchievements/v1/")]
        public Task<ApiResponse<SteamPlayerAchievementsResponse>> GetPlayerAchievements(string key, string steamid, int appid, CancellationToken ct);
    }
}
