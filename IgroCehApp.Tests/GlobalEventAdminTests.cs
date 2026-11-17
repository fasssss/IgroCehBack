using System.Net;
using System.Net.Http.Headers;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // the administrator of the site makes a global event: its games of the catalog with the achievements that prove their passing
    public class GlobalEventAdminTests : GlobalEventTest
    {
        public GlobalEventAdminTests(ApiFactory factory) : base(factory)
        {
        }

        [Fact]
        public async Task Create_TheAchievementsAreKeptWithTheirNames_TheGamesAreSavedFromTheCatalog()
        {
            var created = (await As(Boss).Post("/api/createGlobalEvent", NewEvent(" Ladder ", description: " Line one\r\nLine two "))).Ok().At("globalEvent");

            Assert.Equal(("Ladder", "Line one\nLine two", KnightBadge), (created.Str("name"), created.Str("description"), created.Str("badgeId")));
            Assert.Null(created.At("finishedAt"));
            Assert.Equal((0, 0), (created.Int("participantCount"), created.Int("passedCount")));
            Assert.Equal(new[] { 10, 20, 30 }, created.Ints("games", "appId"));
            var ten = created.At("games[0]");
            Assert.Equal(("Ten", "https://cdn.steam.test/10.jpg"), (ten.Str("game.name"), ten.Str("game.imageUrl")));
            var achievement = Assert.Single(ten.Arr("achievements"));
            Assert.Equal((1, "A2", "A2 name", "A2 имя", "A2 name: description", "https://steam.test/icons/A2.jpg"),
                (achievement.Int("platform"), achievement.Str("apiName"), achievement.Str("name"), achievement.Str("nameRu"), achievement.Str("description"), achievement.Str("iconUrl")));
            Assert.Empty(ten.Arr("titles"));
            Assert.Empty(created.Arr("games[2].achievements"));
            Assert.Equal(3, await Factory.ScalarAsync("SELECT COUNT(*) FROM Games WHERE VgcId IS NOT NULL"));
        }

        [Fact]
        public async Task Create_WhileAnotherIsActive_IsRefused()
        {
            await CreateGlobalEvent();

            IsConflict(await As(Boss).Post("/api/createGlobalEvent", NewEvent("Second")), "Another global event is active");
        }

        [Fact]
        public async Task Create_IsChecked()
        {
            Assert.Equal("There is no such badge", (await As(Boss).Post("/api/createGlobalEvent", NewEvent(badgeId: "unknown"))).Is(HttpStatusCode.BadRequest).Text);
            Assert.Equal("A game has no such achievement on the platform",
                (await As(Boss).Post("/api/createGlobalEvent", NewEvent(games: new[] { SteamGame(10, "A9"), SteamGame(20), SteamGame(30) }))).Is(HttpStatusCode.BadRequest).Text);
            // the game outside Steam has no Steam achievements
            Assert.Equal("A game has no such achievement on the platform",
                (await As(Boss).Post("/api/createGlobalEvent", NewEvent(games: new[] { SteamGame(1_000_000_001, "A1"), SteamGame(20), SteamGame(30) }))).Is(HttpStatusCode.BadRequest).Text);
            Assert.Equal("A game isn't in the catalog",
                (await As(Boss).Post("/api/createGlobalEvent", NewEvent(games: new[] { SteamGame(10), SteamGame(20), SteamGame(77) }))).Is(HttpStatusCode.BadRequest).Text);
            Assert.Contains("A player rolls 3 games",
                (await As(Boss).Post("/api/createGlobalEvent", NewEvent(games: new[] { SteamGame(10), SteamGame(20) }))).Is(HttpStatusCode.BadRequest).Body);
            Assert.Contains("A game is added once",
                (await As(Boss).Post("/api/createGlobalEvent", NewEvent(games: new[] { SteamGame(10), SteamGame(10), SteamGame(20) }))).Is(HttpStatusCode.BadRequest).Body);
            Assert.Contains("The event needs a name", (await As(Boss).Post("/api/createGlobalEvent", NewEvent(" "))).Is(HttpStatusCode.BadRequest).Body);
            Assert.Contains("The event needs a badge", (await As(Boss).Post("/api/createGlobalEvent", NewEvent(badgeId: ""))).Is(HttpStatusCode.BadRequest).Body);
            Assert.Equal(0, await Factory.ScalarAsync("SELECT COUNT(*) FROM GlobalEvents"));
        }

        [Fact]
        public async Task Create_WhenSteamFails_Is503()
        {
            External.OnGet($"{ApiFactory.SteamAddress}/ISteamUserStats/GetSchemaForGame/v2/", new { }, HttpStatusCode.InternalServerError);

            (await As(Boss).Post("/api/createGlobalEvent", NewEvent())).Is(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task AdminEndpoints_AreForTheAdministratorOfTheSiteOnly()
        {
            var id = await CreateGlobalEvent();

            (await As(Dayt).Post("/api/createGlobalEvent", NewEvent())).Is(HttpStatusCode.Forbidden);
            (await As(Dayt).Post("/api/updateGlobalEvent", new { globalEventId = id })).Is(HttpStatusCode.Forbidden);
            (await As(Dayt).Post("/api/finishGlobalEvent", new { globalEventId = id })).Is(HttpStatusCode.Forbidden);
            (await As(Dayt).Post("/api/deleteGlobalEvent", new { globalEventId = id })).Is(HttpStatusCode.Forbidden);
            (await As(Dayt).Get($"/api/getGlobalEventClaims?globalEventId={id}")).Is(HttpStatusCode.Forbidden);
            (await As(Dayt).Get("/api/getBadges")).Is(HttpStatusCode.Forbidden);
            (await As(Dayt).Get("/api/getSteamAchievements?appId=10")).Is(HttpStatusCode.Forbidden);
            (await As(Dayt).Get("/api/getPsnStatus")).Is(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Active_IsTheUnfinishedOne_ForEveryPlayer()
        {
            Assert.Null((await As(Dayt).Get("/api/getActiveGlobalEvent")).Ok().At("globalEvent"));
            var id = await CreateGlobalEvent();

            Assert.Equal(id, (await As(Dayt).Get("/api/getActiveGlobalEvent")).Ok().Str("globalEvent.id"));
            Assert.Equal("Ladder", (await As(Nico).Get($"/api/getGlobalEvent?globalEventId={id}")).Ok().Str("globalEvent.name"));
            (await As(Nico).Get("/api/getGlobalEvent?globalEventId=unknown")).Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Finish_Once_ThenANewOneCanBeCreated()
        {
            var id = await CreateGlobalEvent();

            (await As(Boss).Post("/api/finishGlobalEvent", new { globalEventId = id })).Ok();
            var again = await As(Boss).Post("/api/finishGlobalEvent", new { globalEventId = id });

            Assert.Equal("The event is finished already", again.Is(HttpStatusCode.BadRequest).Text);
            Assert.Null((await As(Dayt).Get("/api/getActiveGlobalEvent")).Ok().At("globalEvent"));
            Assert.NotNull((await As(Dayt).Get($"/api/getGlobalEvent?globalEventId={id}")).Ok().Date("globalEvent.finishedAt"));
            (await As(Boss).Post("/api/finishGlobalEvent", new { globalEventId = "unknown" })).Is(HttpStatusCode.NotFound);
            await CreateGlobalEvent(NewEvent("Next"));
        }

        [Fact]
        public async Task Delete_HidesTheEvent()
        {
            var id = await CreateGlobalEvent();

            (await As(Boss).Post("/api/deleteGlobalEvent", new { globalEventId = id })).Ok();

            (await As(Dayt).Get($"/api/getGlobalEvent?globalEventId={id}")).Is(HttpStatusCode.NotFound);
            Assert.Null((await As(Dayt).Get("/api/getActiveGlobalEvent")).Ok().At("globalEvent"));
            (await As(Boss).Post("/api/deleteGlobalEvent", new { globalEventId = id })).Is(HttpStatusCode.NotFound);
            Assert.Equal(1, await Factory.ScalarAsync($"SELECT COUNT(*) FROM GlobalEvents WHERE Id = '{id}' AND IsDeleted AND DeletedById = '{Boss}'"));
        }

        [Fact]
        public async Task Update_NameAndOrder_WithoutAskingThePlatforms()
        {
            var id = await CreateGlobalEvent();
            var steamRequests = External.Requests.Count(r => r.Address.StartsWith(ApiFactory.SteamAddress));

            var updated = (await As(Boss).Post("/api/updateGlobalEvent", new
            {
                globalEventId = id,
                name = "Renamed",
                description = "",
                badgeId = WheelBadge,
                games = new[] { SteamGame(30), SteamGame(20, "B1"), SteamGame(10, "A2") },
            })).Ok().At("globalEvent");

            Assert.Equal(("Renamed", null, WheelBadge), (updated.Str("name"), updated.Str("description"), updated.Str("badgeId")));
            Assert.Equal(new[] { 30, 20, 10 }, updated.Ints("games", "appId"));
            Assert.Equal(steamRequests, External.Requests.Count(r => r.Address.StartsWith(ApiFactory.SteamAddress)));
        }

        [Fact]
        public async Task Update_TheGamesAndAchievements_UntilSomeoneRolls()
        {
            var id = await CreateGlobalEvent();

            var updated = (await As(Boss).Post("/api/updateGlobalEvent", new
            {
                globalEventId = id,
                name = "Ladder",
                badgeId = KnightBadge,
                games = new[] { SteamGame(10, "A1", "A2"), SteamGame(20), SteamGame(40) },
            })).Ok().At("globalEvent");
            (await As(Dayt).Post("/api/rollGlobalEventGames", new { globalEventId = id })).Ok();
            var locked = await As(Boss).Post("/api/updateGlobalEvent", new
            {
                globalEventId = id,
                name = "Ladder",
                badgeId = KnightBadge,
                games = new[] { SteamGame(10, "A1"), SteamGame(20), SteamGame(40) },
            });

            Assert.Equal(new[] { 10, 20, 40 }, updated.Ints("games", "appId"));
            Assert.Equal(new[] { "A1", "A2" }, updated.Strs("games[0].achievements", "apiName"));
            Assert.Empty(updated.Arr("games[1].achievements"));
            IsConflict(locked, "Players have rolled the games, they can't change");
        }

        [Fact]
        public async Task Update_TheBadge_UntilSomeonePasses()
        {
            var id = await CreateGlobalEvent();
            await Pick(id, Dayt, 10);
            await Db(async db =>
            {
                db.GlobalEventParticipants.Single().PassedAt = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
            });

            var locked = await As(Boss).Post("/api/updateGlobalEvent", new { globalEventId = id, name = "Ladder", badgeId = WheelBadge, games = new[] { SteamGame(10, "A2"), SteamGame(20, "B1"), SteamGame(30) } });

            IsConflict(locked, "Players have got the badge, it can't change");
            (await As(Boss).Post("/api/updateGlobalEvent", new { globalEventId = "unknown", name = "X", badgeId = KnightBadge, games = new[] { SteamGame(10), SteamGame(20), SteamGame(30) } }))
                .Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Image_IsUploadedAndReplaced()
        {
            var id = await CreateGlobalEvent();
            MultipartFormDataContent Form(string globalEventId, byte[] image, string contentType)
            {
                var file = new ByteArrayContent(image);
                file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                return new MultipartFormDataContent { { new StringContent(globalEventId), "globalEventId" }, { file, "image", "banner" } };
            }

            var first = (await As(Boss).PostForm("/api/setGlobalEventImage", Form(id, new byte[] { 1, 2, 3 }, "image/webp"))).Ok().Str("imageUrl")!;
            var second = (await As(Boss).PostForm("/api/setGlobalEventImage", Form(id, new byte[] { 4, 5 }, "image/jpeg"))).Ok().Str("imageUrl")!;

            Assert.Matches("^/Upload/GlobalEvents/[0-9a-f]{32}\\.webp$", first);
            Assert.EndsWith(".jpg", second);
            Assert.Equal(second, (await As(Dayt).Get($"/api/getGlobalEvent?globalEventId={id}")).Ok().Str("globalEvent.imageUrl"));
            Assert.Equal(new byte[] { 4, 5 }, (await Anonymous.Get(second)).Is(HttpStatusCode.OK).Bytes);
            (await As(Boss).PostForm("/api/setGlobalEventImage", Form("unknown", new byte[] { 1 }, "image/png"))).Is(HttpStatusCode.NotFound);
            (await As(Boss).PostForm("/api/setGlobalEventImage", Form(id, new byte[] { 1 }, "image/svg+xml"))).Is(HttpStatusCode.BadRequest);
            (await As(Boss).PostForm("/api/setGlobalEventImage", Form(id, Array.Empty<byte>(), "image/png"))).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task SteamAchievements_ForTheConstructor()
        {
            var achievements = (await As(Boss).Get("/api/getSteamAchievements?appId=10")).Ok().Arr("achievements");
            var outside = (await As(Boss).Get("/api/getSteamAchievements?appId=1000000001")).Ok().Arr("achievements");
            (await As(Boss).Get("/api/getSteamAchievements?appId=10")).Ok();

            Assert.Equal(new[] { ("A1", 50.0, false), ("A2", 49.0, false) },
                achievements.Select(a => (a.Str("apiName"), a.Num("percent")!.Value, a.Bool("isHidden"))));
            Assert.Equal(("A1 name", "A1 имя", "A1 имя: описание"), (achievements[0].Str("name"), achievements[0].Str("nameRu"), achievements[0].Str("descriptionRu")));
            Assert.Empty(outside);
            // the achievements are cached, Steam is asked once in each language
            Assert.Equal(2, External.CountOf($"{ApiFactory.SteamAddress}/ISteamUserStats/GetSchemaForGame/v2/"));
            Assert.Contains("key=test-steam-key", External.Requests.First().Query);
        }

        [Fact]
        public async Task SteamAchievements_WhenSteamFails_Is503()
        {
            External.OnGet($"{ApiFactory.SteamAddress}/ISteamUserStats/GetSchemaForGame/v2/", new { }, HttpStatusCode.ServiceUnavailable);

            (await As(Boss).Get("/api/getSteamAchievements?appId=10")).Is(HttpStatusCode.ServiceUnavailable);
        }
    }
}
