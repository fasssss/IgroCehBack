using System.Net;
using Domain.Enums;
using IgroCehApp.Tests.Infrastructure;

namespace IgroCehApp.Tests
{
    // the profile of a player as another player sees it: the guilds they share, the statistics, the history of the events and the tastes
    public class PlayerTests : GuildTest
    {
        private const string G3 = "g3";
        private const string Other = "other";

        public PlayerTests(ApiFactory factory) : base(factory)
        {
        }

        // Dayt has played in g1 with Sliva and Nico and in g3 with Other, Sliva isn't in g3
        private async Task SeedHistory()
        {
            var lastSeason = Seasons.CurrentStart().AddMonths(-3);
            var season = Seasons.CurrentStart();
            await Db(async db =>
            {
                db.Games.Find(Elden)!.Platforms = new List<string> { "PC", "PS5" };
                db.Games.Find(Hollow)!.Platforms = new List<string> { "PC", "Switch" };
                db.Games.Find(Sekiro)!.Platforms = new List<string> { "PC" };
                await db.SaveChangesAsync();
            });
            DateTimeOffset At(DateTime start, int days) => new DateTimeOffset(start, TimeSpan.Zero).AddDays(days);
            await Seed(Make.User(Other, "Other"), Make.Guild(G3, "Third"), Make.Member(Dayt, G3), Make.Member(Other, G3),
                Make.Event("e1", G1, Sliva, EventStatusId.Finished, lastSeason, name: "Last season"),
                Make.Record("e1-r0", "e1", Sliva, Dayt, Elden, 3, At(lastSeason, 10)),
                Make.Record("e1-r1", "e1", Dayt, Nico, Hollow),
                Make.Record("e1-r2", "e1", Nico, Sliva, Sekiro, 2, At(lastSeason, 40)),
                Make.Event("e2", G1, Sliva, EventStatusId.Active, season, name: "This season"),
                Make.Record("e2-r0", "e2", Dayt, Sliva, Persona),
                Make.Record("e2-r1", "e2", Sliva, Dayt, Hollow, 1, At(season, 70)),
                Make.Event("e3", G3, Dayt, EventStatusId.Finished, lastSeason, name: "Elsewhere"),
                Make.Record("e3-r0", "e3", Dayt, Other, Persona, 3, At(lastSeason, 20)),
                Make.Record("e3-r1", "e3", Other, Dayt, Sekiro, 2, At(lastSeason, 5)));
        }

        [Fact]
        public async Task Profile_TotalsOfEveryGuild_GuildsOnlyTheSharedOnes()
        {
            await SeedHistory();

            var profile = (await As(Sliva).Get($"/api/getPlayerProfile?userId={Dayt}")).Ok().At("playerProfile");

            Assert.Equal((Dayt, "Dayt", "https://cdn.test/avatars/dayt.png"), (profile.Str("id"), profile.Str("userName"), profile.Str("avatarUrl")));
            Assert.NotNull(profile.Date("playingSince"));
            var guild = Assert.Single(profile.Arr("guilds"));
            Assert.Equal((G1, "Guild One", 4, 1, 5, false), (guild.Str("id"), guild.Str("name"), guild.Int("score"), guild.Int("place"), guild.Int("playersCount"), guild.Bool("hasLeft")));
            Assert.Equal(1, profile.Int("otherGuildsCount"));
            var statistics = profile.At("statistics");
            Assert.Equal(6, statistics.Int("score"));
            Assert.Equal(3, statistics.Int("eventsPlayed"));
            Assert.Equal(1, statistics.Int("eventsInProgress"));
            Assert.Equal(3, statistics.Int("gamesCompleted"));
            Assert.Equal(0, statistics.Int("gamesFailed"));
            Assert.Equal(28.3, statistics.Num("averageDaysToComplete"));
            Assert.Equal(3, statistics.Int("gamesSuggested"));
            Assert.Equal(1, statistics.Int("suggestedGamesCompleted"));
            Assert.Equal(1, statistics.Int("suggestedGamesFailed"));
            Assert.Equal(new[] { (3, 1), (2, 1), (1, 1), (0, 0) }, statistics.Arr("rewards").Select(r => (r.Int("reward"), r.Int("count"))));
            Assert.Equal(new[] { ("PC", 3), ("PS5", 1), ("Switch", 1) }, profile.Arr("favoritePlatforms").Select(p => (p.Str("platform"), p.Int("count"))));
            Assert.Empty(profile.Arr("badges"));
        }

        [Fact]
        public async Task Profile_IsOpenedOnlyThroughASharedGuild()
        {
            await SeedHistory();

            (await As(Stranger).Get($"/api/getPlayerProfile?userId={Dayt}")).Is(HttpStatusCode.NotFound);
            (await As(Sliva).Get("/api/getPlayerProfile?userId=unknown")).Is(HttpStatusCode.NotFound);
            // a shared guild opens it even if the player hasn't played
            var admin = (await As(Sliva).Get($"/api/getPlayerProfile?userId={Admin}")).Ok().At("playerProfile");
            Assert.Empty(admin.Arr("guilds"));
            Assert.Equal(0, admin.Int("statistics.eventsPlayed"));
            Assert.Null(admin.At("statistics.averageDaysToComplete"));
        }

        [Fact]
        public async Task Profile_GivesTheLinkedAccounts()
        {
            await Db(async db =>
            {
                var user = db.Users.Find(Dayt)!;
                user.SteamId = "76561198000000002";
                user.XboxGamertag = "DaytXbox";
                user.PsnOnlineId = "DaytPsn";
                await db.SaveChangesAsync();
            });

            var profile = (await As(Sliva).Get($"/api/getPlayerProfile?userId={Dayt}")).Ok().At("playerProfile");

            Assert.Equal(("76561198000000002", "DaytXbox", "DaytPsn"), (profile.Str("steamId"), profile.Str("xboxGamertag"), profile.Str("psnOnlineId")));
        }

        [Fact]
        public async Task History_EventsOfTheGuildsOfTheViewer_TheLatestSeasonFirst()
        {
            await SeedHistory();

            var history = (await As(Sliva).Get($"/api/getPlayerHistory?userId={Dayt}&startFrom=0")).Ok();

            Assert.False(history.Bool("hasMore"));
            var items = history.Arr("historyItems");
            Assert.Equal(new[] { "e2", "e1" }, items.Select(i => i.Str("eventId")));
            var current = items[0];
            Assert.Equal(("This season", G1, "Guild One", (int)EventStatusId.Active), (current.Str("eventName"), current.Str("guildId"), current.Str("guildName"), current.Int("statusId")));
            Assert.Equal(("Hollow Knight", Sliva, 1), (current.Str("receivedGame.game.name"), current.Str("receivedGame.partner.id"), current.Int("receivedGame.reward")));
            Assert.Equal(("Persona 5", Sliva), (current.Str("suggestedGame.game.name"), current.Str("suggestedGame.partner.id")));
            Assert.Null(current.At("suggestedGame.reward"));
            Assert.False(current.Bool("isNewcomer"));
            Assert.Equal(new[] { "e3" }, (await As(Other).Get($"/api/getPlayerHistory?userId={Dayt}&startFrom=0")).Ok().Strs("historyItems", "eventId"));
            Assert.Empty((await As(Stranger).Get($"/api/getPlayerHistory?userId={Dayt}&startFrom=0")).Ok().Arr("historyItems"));
            Assert.Equal(new[] { "e1" }, (await As(Sliva).Get($"/api/getPlayerHistory?userId={Dayt}&startFrom=1")).Ok().Strs("historyItems", "eventId"));
        }

        [Fact]
        public async Task History_IsGivenByTenEvents()
        {
            var season = Seasons.CurrentStart();
            await Seed(Enumerable.Range(0, 11).SelectMany(i => new object[]
            {
                Make.Event($"e{i:00}", G1, Sliva, EventStatusId.Finished, season.AddMonths(-3 * i)),
                Make.Record($"e{i:00}-r0", $"e{i:00}", Dayt, Sliva),
            }).ToArray());

            var first = (await As(Sliva).Get($"/api/getPlayerHistory?userId={Dayt}&startFrom=0")).Ok();
            var second = (await As(Sliva).Get($"/api/getPlayerHistory?userId={Dayt}&startFrom=10")).Ok();

            Assert.Equal(10, first.Arr("historyItems").Count);
            Assert.True(first.Bool("hasMore"));
            Assert.Equal(new[] { "e10" }, second.Strs("historyItems", "eventId"));
            Assert.False(second.Bool("hasMore"));
        }

        [Fact]
        public async Task History_OfANewcomer()
        {
            await Seed(Make.Event("e1", G1, Sliva, EventStatusId.Active, allowsNewcomers: true), Make.Record("e1-r0", "e1", Sliva, Dayt, Elden),
                Make.Record("e1-r1", "e1", Dayt, Sliva, Hollow), Make.PoolGame("p1", "e1", Sekiro, Sliva),
                Make.Record("e1-new", "e1", Sliva, Nico, Sekiro, poolGameId: "p1"));

            var nico = Assert.Single((await As(Sliva).Get($"/api/getPlayerHistory?userId={Nico}&startFrom=0")).Ok().Arr("historyItems"));
            var sliva = Assert.Single((await As(Nico).Get($"/api/getPlayerHistory?userId={Sliva}&startFrom=0")).Ok().Arr("historyItems"));

            Assert.True(nico.Bool("isNewcomer"));
            Assert.Equal(("Sekiro", Sliva), (nico.Str("receivedGame.game.name"), nico.Str("receivedGame.partner.id")));
            Assert.Null(nico.At("suggestedGame"));
            // the author of the game of the pool suggested the game of the chain, not the one of the pool
            Assert.Equal(("Elden Ring", Dayt), (sliva.Str("suggestedGame.game.name"), sliva.Str("suggestedGame.partner.id")));
        }

        [Fact]
        public async Task Tags_AreTheSeededOnes()
        {
            var tags = (await As(Sliva).Get("/api/getPreferenceTags")).Ok().Arr("tags");

            Assert.Equal(await Factory.ScalarAsync("SELECT COUNT(*) FROM PreferenceTags"), tags.Count);
            Assert.Equal(("Action", "Экшен"), (tags[0].Str("name"), tags[0].Str("nameRu")));
            Assert.Equal(tags.Select(t => t.Int("id")).OrderBy(id => id), tags.Select(t => t.Int("id")));
        }

        [Fact]
        public async Task Preferences_AreSavedInTheirOrder_AndReplacedAsAWhole()
        {
            (await As(Dayt).Post("/api/setPreferences", new { preferredTagIds = new[] { 3, 1 }, avoidedTagIds = new[] { 7 } })).Ok();
            (await As(Dayt).Post("/api/setPreferences", new { preferredTagIds = new[] { 2, 3 }, avoidedTagIds = new[] { 1 } })).Ok();

            var preferences = (await As(Sliva).Get($"/api/getPlayerPreferences?userId={Dayt}")).Ok();

            Assert.Equal(new[] { 2, 3 }, preferences.Ints("preferred", "id"));
            Assert.Equal(new[] { 1 }, preferences.Ints("avoided", "id"));
            Assert.Equal("Adventure", preferences.Str("preferred[0].name"));
        }

        [Fact]
        public async Task Preferences_AreSeenByTheSameViewersAsTheProfile()
        {
            (await As(Stranger).Get($"/api/getPlayerPreferences?userId={Dayt}")).Is(HttpStatusCode.NotFound);
            (await As(Stranger).Get($"/api/getPlayerPreferences?userId={Stranger}")).Is(HttpStatusCode.OK);
            (await As(Sliva).Get("/api/getPlayerPreferences?userId=unknown")).Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Preferences_AreChecked()
        {
            Assert.Equal("There is no such tag",
                (await As(Dayt).Post("/api/setPreferences", new { preferredTagIds = new[] { 100000 }, avoidedTagIds = Array.Empty<int>() })).Is(HttpStatusCode.BadRequest).Text);
            (await As(Dayt).Post("/api/setPreferences", new { preferredTagIds = new[] { 1 }, avoidedTagIds = new[] { 1 } })).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/setPreferences", new { preferredTagIds = new[] { 1, 1 }, avoidedTagIds = Array.Empty<int>() })).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/setPreferences", new { preferredTagIds = Enumerable.Range(1, 21).ToArray(), avoidedTagIds = Array.Empty<int>() })).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/setPreferences", new { preferredTagIds = new[] { 1 } })).Is(HttpStatusCode.BadRequest);
            Assert.Equal(0, await Factory.ScalarAsync("SELECT COUNT(*) FROM UserPreferences"));
        }
    }
}
