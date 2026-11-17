using System.Net;
using System.Web;

namespace IgroCehApp.Tests.Infrastructure
{
    // the Steam Web API as the fake answers it: the achievements of games and the games and achievements of players;
    // Steam keeps every game behind one address, so the answers are chosen by the query
    public class FakeSteam
    {
        private readonly Dictionary<int, List<(string ApiName, string Name, string NameRu, double Percent)>> _games = new();
        private readonly Dictionary<string, HashSet<int>?> _owned = new();
        private readonly Dictionary<(string SteamId, int AppId), Dictionary<string, DateTimeOffset>?> _unlocked = new();

        public FakeSteam(FakeExternalApis apis)
        {
            apis.On(HttpMethod.Get, $"{ApiFactory.SteamAddress}/ISteamUserStats/GetSchemaForGame/v2/", (request, _) =>
            {
                var query = HttpUtility.ParseQueryString(request.RequestUri!.Query);
                var achievements = _games.GetValueOrDefault(int.Parse(query["appid"]!)) ?? new();
                var isRussian = query["l"] == "russian";
                return FakeExternalApis.Json(new
                {
                    game = new
                    {
                        gameName = "Game",
                        availableGameStats = new
                        {
                            achievements = achievements.Select(a => new
                            {
                                name = a.ApiName,
                                displayName = isRussian ? a.NameRu : a.Name,
                                description = isRussian ? $"{a.NameRu}: описание" : $"{a.Name}: description",
                                icon = $"https://steam.test/icons/{a.ApiName}.jpg",
                                hidden = 0,
                            }),
                        },
                    },
                });
            });
            apis.On(HttpMethod.Get, $"{ApiFactory.SteamAddress}/ISteamUserStats/GetGlobalAchievementPercentagesForApp/v2/", (request, _) =>
            {
                var appId = int.Parse(HttpUtility.ParseQueryString(request.RequestUri!.Query)["gameid"]!);
                var achievements = _games.GetValueOrDefault(appId) ?? new();
                return FakeExternalApis.Json(new { achievementpercentages = new { achievements = achievements.Select(a => new { name = a.ApiName, percent = a.Percent.ToString(System.Globalization.CultureInfo.InvariantCulture) }) } });
            });
            apis.On(HttpMethod.Get, $"{ApiFactory.SteamAddress}/IPlayerService/GetOwnedGames/v1/", (request, _) =>
            {
                var steamId = HttpUtility.ParseQueryString(request.RequestUri!.Query)["steamid"]!;
                // a profile with hidden game details answers without the count
                return _owned.TryGetValue(steamId, out var owned) && owned != null
                    ? FakeExternalApis.Json(new { response = new { game_count = owned.Count, games = owned.Select(appid => new { appid }) } })
                    : FakeExternalApis.Json(new { response = new { } });
            });
            apis.On(HttpMethod.Get, $"{ApiFactory.SteamAddress}/ISteamUserStats/GetPlayerAchievements/v1/", (request, _) =>
            {
                var query = HttpUtility.ParseQueryString(request.RequestUri!.Query);
                var key = (query["steamid"]!, int.Parse(query["appid"]!));
                if (!_unlocked.TryGetValue(key, out var unlocked))
                {
                    return FakeExternalApis.Json(new { playerstats = new { error = "Requested app has no stats", success = false } }, HttpStatusCode.BadRequest);
                }

                if (unlocked == null)
                {
                    return FakeExternalApis.Json(new { playerstats = new { error = "Profile is not public", success = false } }, HttpStatusCode.Forbidden);
                }

                var all = (_games.GetValueOrDefault(key.Item2) ?? new()).Select(a => a.ApiName).Union(unlocked.Keys);
                return FakeExternalApis.Json(new
                {
                    playerstats = new
                    {
                        achievements = all.Select(apiName => new
                        {
                            apiname = apiName,
                            achieved = unlocked.ContainsKey(apiName) ? 1 : 0,
                            unlocktime = unlocked.TryGetValue(apiName, out var at) ? at.ToUnixTimeSeconds() : 0,
                        }),
                    },
                });
            });
        }

        public FakeSteam Game(int appId, params string[] apiNames)
        {
            _games[appId] = apiNames.Select((apiName, index) => (apiName, $"{apiName} name", $"{apiName} имя", 50.04 - index)).ToList();
            return this;
        }

        public FakeSteam Owns(string steamId, params int[] appIds)
        {
            _owned[steamId] = appIds.ToHashSet();
            return this;
        }

        public FakeSteam HidesGames(string steamId)
        {
            _owned[steamId] = null;
            return this;
        }

        public FakeSteam Unlocked(string steamId, int appId, params (string ApiName, DateTimeOffset At)[] achievements)
        {
            _unlocked[(steamId, appId)] = achievements.ToDictionary(a => a.ApiName, a => a.At);
            return this;
        }

        public FakeSteam HidesAchievements(string steamId, int appId)
        {
            _unlocked[(steamId, appId)] = null;
            return this;
        }
    }
}
