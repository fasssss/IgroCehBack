using System.Net;

namespace IgroCehApp.Tests.Infrastructure
{
    // a test of the global events: Boss is the administrator of the site, Dayt and Nico play; the catalog knows three Steam games,
    // 10 with the achievements A1 and A2, 20 with B1 and 30 without achievements
    public abstract class GlobalEventTest : ApiTest
    {
        protected const string Boss = "boss";
        protected const string Dayt = "dayt";
        protected const string Nico = "nico";
        protected const string KnightBadge = "knight-badge";
        protected const string WheelBadge = "wheel-badge";

        protected GlobalEventTest(ApiFactory factory) : base(factory)
        {
        }

        protected FakeSteam Steam { get; private set; } = null!;

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await Seed(Make.User(Boss, "Boss", isAppAdmin: true), Make.User(Dayt, "Dayt"), Make.User(Nico, "Nico"));
            Steam = new FakeSteam(External).Game(10, "A1", "A2").Game(20, "B1").Game(30);
            foreach (var (appId, name) in new[] { (10, "Ten"), (20, "Twenty"), (30, "Thirty"), (40, "Forty") })
            {
                CatalogKnows(appId, name);
            }
        }

        protected void CatalogKnows(int appId, string name) =>
            External.OnGet($"{ApiFactory.CatalogAddress}/api/v1/game/{appId}", new
            {
                id = appId,
                name,
                platforms = new[] { "PC" },
                art = new { cover = $"https://cdn.steam.test/{appId}.jpg" },
            });

        protected static object SteamGame(int appId, params string[] achievementIds) => new
        {
            appId,
            platforms = achievementIds.Length == 0
                ? Array.Empty<object>()
                : new object[] { new { platform = 1, titleRef = (string?)null, achievementIds } },
        };

        protected static object NewEvent(string name = "Ladder", string badgeId = KnightBadge, object[]? games = null, string? description = null) => new
        {
            name,
            description,
            badgeId,
            games = games ?? new[] { SteamGame(10, "A2"), SteamGame(20, "B1"), SteamGame(30) },
        };

        protected async Task<string> CreateGlobalEvent(object? request = null) =>
            (await As(Boss).Post("/api/createGlobalEvent", request ?? NewEvent())).Ok().Str("globalEvent.id")!;

        // the player rolls the three games and picks the given one
        protected async Task Pick(string globalEventId, string player, int appId)
        {
            (await As(player).Post("/api/rollGlobalEventGames", new { globalEventId })).Ok();
            (await As(player).Post("/api/pickGlobalEventGame", new { globalEventId, appId })).Ok();
        }

        protected Task LinkSteam(string userId, string steamId) => Db(async db =>
        {
            db.Users.Find(userId)!.SteamId = steamId;
            await db.SaveChangesAsync();
        });

        protected static void IsConflict(ApiResult answer, string text) => Assert.Equal(text, answer.Is(HttpStatusCode.Conflict).Text);
    }
}
