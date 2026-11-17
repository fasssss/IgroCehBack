using System.Net;
using Domain.Enums;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // the pool for newcomers: the players fill it during the registration, a newcomer joins the active event and rolls a game of it
    public class NewcomerPoolTests : GuildTest
    {
        private const string Rival = "rival";

        public NewcomerPoolTests(ApiFactory factory) : base(factory)
        {
        }

        private Task<ApiResult> Add(string by, string gameId, string eventId = "e1") =>
            As(by).Post("/api/addNewcomerPoolGame", new { eventId, gameId });

        [Fact]
        public async Task Pool_IsSeenByTheGuild_WithoutTheAuthors()
        {
            await SeedEvent("e1", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt }, allowsNewcomers: true);
            await Seed(Make.PoolGame("p1", "e1", Elden, Sliva, DateTimeOffset.UtcNow.AddMinutes(-2)), Make.PoolGame("p2", "e1", Hollow, Dayt, DateTimeOffset.UtcNow));

            var forSliva = (await As(Sliva).Get("/api/getNewcomerPool?eventId=e1")).Ok();
            var forNico = (await As(Nico).Get("/api/getNewcomerPool?eventId=e1")).Ok();

            Assert.Equal(new[] { ("p1", "Elden Ring", true), ("p2", "Hollow Knight", false) },
                forSliva.Arr("games").Select(g => (g.Str("id"), g.Str("game.name"), g.Bool("isMine"))));
            Assert.All(forNico.Arr("games"), g => Assert.False(g.Bool("isMine")));
            Assert.Equal((3, 1), (forNico.Int("minGames"), forNico.Int("gamesPerPlayer")));
            Assert.DoesNotContain(Sliva, forNico.Compact());
        }

        [Fact]
        public async Task Pool_IsHidden_FromStrangers_AndInEventsWithoutNewcomers()
        {
            await SeedEvent("e1", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt }, allowsNewcomers: true);
            await SeedEvent("e2", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt });

            (await As(Stranger).Get("/api/getNewcomerPool?eventId=e1")).Is(HttpStatusCode.NotFound);
            (await As(Sliva).Get("/api/getNewcomerPool?eventId=e2")).Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Add_APlayerPutsOneGame_EachGameOnce()
        {
            await SeedEvent("e1", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt }, allowsNewcomers: true);

            var added = (await Add(Sliva, Elden)).Ok();
            var second = await Add(Sliva, Hollow);
            var sameGame = await Add(Dayt, Elden);

            Assert.Equal((Elden, true), (added.Str("poolGame.game.id"), added.Bool("poolGame.isMine")));
            Assert.Equal("You have put as many games as one may", second.Is(HttpStatusCode.BadRequest).Text);
            Assert.Equal("The game is already in the pool", sameGame.Is(HttpStatusCode.Conflict).Text);
        }

        [Fact]
        public async Task Add_IsRefused()
        {
            await SeedEvent("e1", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt }, allowsNewcomers: true);
            await SeedEvent("e2", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt });
            await SeedEvent("e3", EventStatusId.PlayersShuffle, new[] { Sliva, Dayt }, allowsNewcomers: true);

            (await Add(Nico, Elden)).Is(HttpStatusCode.Forbidden);
            (await Add(Sliva, "unknown")).Is(HttpStatusCode.NotFound);
            (await Add(Sliva, Elden, "unknown")).Is(HttpStatusCode.NotFound);
            Assert.Equal("The pool can't be changed now", (await Add(Sliva, Elden, "e2")).Is(HttpStatusCode.BadRequest).Text);
            (await Add(Sliva, Elden, "e3")).Is(HttpStatusCode.BadRequest);
            Assert.Contains("eventId", (await As(Sliva).Post("/api/addNewcomerPoolGame", new { gameId = Elden })).Is(HttpStatusCode.BadRequest).Body,
                StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public async Task Replace_AndRemove_ByTheAuthorDuringTheRegistration()
        {
            await SeedEvent("e1", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt }, allowsNewcomers: true);
            await Seed(Make.PoolGame("p1", "e1", Elden, Sliva), Make.PoolGame("p2", "e1", Hollow, Dayt));

            Assert.Equal(Sekiro, (await As(Sliva).Post("/api/replaceNewcomerPoolGame", new { eventId = "e1", poolGameId = "p1", gameId = Sekiro })).Ok().Str("poolGame.game.id"));
            (await As(Sliva).Post("/api/replaceNewcomerPoolGame", new { eventId = "e1", poolGameId = "p1", gameId = Sekiro })).Ok();
            Assert.Equal("The game is already in the pool",
                (await As(Sliva).Post("/api/replaceNewcomerPoolGame", new { eventId = "e1", poolGameId = "p1", gameId = Hollow })).Is(HttpStatusCode.Conflict).Text);
            (await As(Dayt).Post("/api/replaceNewcomerPoolGame", new { eventId = "e1", poolGameId = "p1", gameId = Persona })).Is(HttpStatusCode.Forbidden);
            (await As(Sliva).Post("/api/replaceNewcomerPoolGame", new { eventId = "e1", poolGameId = "p1", gameId = "unknown" })).Is(HttpStatusCode.NotFound);
            (await As(Dayt).Post("/api/removeNewcomerPoolGame", new { eventId = "e1", poolGameId = "p1" })).Is(HttpStatusCode.Forbidden);
            (await As(Sliva).Post("/api/removeNewcomerPoolGame", new { eventId = "e1", poolGameId = "missing" })).Is(HttpStatusCode.NotFound);
            (await As(Sliva).Post("/api/removeNewcomerPoolGame", new { eventId = "e1", poolGameId = "p1" })).Ok();

            Assert.Equal(new[] { "p2" }, await Db(db => db.NewcomerPoolGames.Select(pg => pg.Id).ToListAsync()));
        }

        [Fact]
        public async Task Replace_AndRemove_AfterTheRegistration_AreRefused()
        {
            await SeedEvent("e1", EventStatusId.Active, new[] { Sliva, Dayt }, new[] { Elden, Hollow }, allowsNewcomers: true);
            await Seed(Make.PoolGame("p1", "e1", Sekiro, Sliva));

            (await As(Sliva).Post("/api/replaceNewcomerPoolGame", new { eventId = "e1", poolGameId = "p1", gameId = Persona })).Is(HttpStatusCode.BadRequest);
            (await As(Sliva).Post("/api/removeNewcomerPoolGame", new { eventId = "e1", poolGameId = "p1" })).Is(HttpStatusCode.BadRequest);
        }

        // e1 is active with newcomers: Sliva, Dayt and Admin are in it, each put one game of three into the pool
        private async Task SeedActivePool()
        {
            await Seed(Make.Game(Rival, Sliva, "Rival"));
            await SeedEvent("e1", EventStatusId.Active, new[] { Sliva, Dayt, Admin }, new[] { Elden, Hollow, Sekiro }, allowsNewcomers: true);
            await Seed(Make.PoolGame("p1", "e1", Persona, Sliva, DateTimeOffset.UtcNow.AddMinutes(-3)),
                Make.PoolGame("p2", "e1", Rival, Dayt, DateTimeOffset.UtcNow.AddMinutes(-2)),
                Make.PoolGame("p3", "e1", Elden, Admin, DateTimeOffset.UtcNow.AddMinutes(-1)));
        }

        [Fact]
        public async Task Roll_ShowsTheGamesANewcomerMayGet()
        {
            await SeedActivePool();

            var roll = (await As(Nico).Get("/api/getNewcomerRoll?eventId=e1")).Ok();

            Assert.Equal(new[] { "p1", "p2", "p3" }, roll.Strs("games", "id"));
            Assert.Equal((0, 3), (roll.Int("hiddenCount"), roll.Int("minGames")));
        }

        [Fact]
        public async Task Roll_HidesTheGamesTheNewcomerHasGot()
        {
            await SeedActivePool();
            await Seed(Make.Event("old", G1, Sliva, EventStatusId.Finished, Seasons.CurrentStart().AddMonths(-3)), Make.Record("old-r0", "old", Sliva, Nico, Persona));

            var roll = (await As(Nico).Get("/api/getNewcomerRoll?eventId=e1")).Ok();

            Assert.Equal(new[] { "p2", "p3" }, roll.Strs("games", "id"));
            Assert.Equal(1, roll.Int("hiddenCount"));
        }

        [Fact]
        public async Task Roll_IsRefused()
        {
            await SeedActivePool();
            await SeedEvent("e2", EventStatusId.GamesGuessing, new[] { Sliva, Dayt }, allowsNewcomers: true);

            Assert.Equal("You are in the event already", (await As(Dayt).Get("/api/getNewcomerRoll?eventId=e1")).Is(HttpStatusCode.Conflict).Text);
            (await As(Stranger).Get("/api/getNewcomerRoll?eventId=e1")).Is(HttpStatusCode.Forbidden);
            (await As(Leaver).Get("/api/getNewcomerRoll?eventId=e1")).Is(HttpStatusCode.Forbidden);
            (await As(Nico).Get("/api/getNewcomerRoll?eventId=unknown")).Is(HttpStatusCode.NotFound);
            Assert.Equal("Newcomers can't join the event now", (await As(Nico).Get("/api/getNewcomerRoll?eventId=e2")).Is(HttpStatusCode.BadRequest).Text);
        }

        [Fact]
        public async Task Join_TheNewcomerGetsAGameOfTheRest_ItsAuthorGuessedIt()
        {
            await SeedActivePool();

            var record = (await As(Nico).Post("/api/joinAsNewcomer", new { eventId = "e1", excludedPoolGameIds = Array.Empty<string>() })).Ok().At("eventRecordObject");

            Assert.True(record.Bool("isNewcomer"));
            Assert.Equal(Nico, record.Str("toUser.id"));
            var authors = new Dictionary<string, string> { [Persona] = Sliva, [Rival] = Dayt, [Elden] = Admin };
            Assert.Equal(authors[record.Str("game.id")!], record.Str("participant.id"));
            var saved = await Db(db => db.EventRecords.SingleAsync(er => er.ToUserId == Nico));
            Assert.NotNull(saved.NewcomerPoolGameId);
            Assert.Equal(Nico, (await GetEvent("e1", Nico)).Arr("eventRecords").Single(r => r.Bool("isNewcomer")).Str("toUser.id"));
            Assert.Equal("You are in the event already",
                (await As(Nico).Post("/api/joinAsNewcomer", new { eventId = "e1", excludedPoolGameIds = Array.Empty<string>() })).Is(HttpStatusCode.Conflict).Text);
        }

        [Fact]
        public async Task Join_CrossedOutGames_AreKeptOutWhileAtLeastThreeStay()
        {
            await SeedActivePool();
            await Seed(Make.Game("fourth", Sliva, "Fourth"), Make.User("p4author", "P4"), Make.Member("p4author", G1), Make.Record("e1-r9", "e1", "p4author"),
                Make.PoolGame("p4", "e1", "fourth", "p4author"));

            var tooMany = await As(Nico).Post("/api/joinAsNewcomer", new { eventId = "e1", excludedPoolGameIds = new[] { "p1", "p2" } });
            var record = (await As(Nico).Post("/api/joinAsNewcomer", new { eventId = "e1", excludedPoolGameIds = new[] { "p1" } })).Ok().At("eventRecordObject");

            Assert.Equal("Too many games are crossed out", tooMany.Is(HttpStatusCode.BadRequest).Text);
            Assert.NotEqual(Persona, record.Str("game.id"));
        }

        [Fact]
        public async Task Join_WithFewerGamesThanTheMinimum_NothingCanBeCrossedOut()
        {
            await SeedEvent("e1", EventStatusId.Active, new[] { Sliva, Dayt }, new[] { Elden, Hollow }, allowsNewcomers: true);
            await Seed(Make.PoolGame("p1", "e1", Sekiro, Sliva), Make.PoolGame("p2", "e1", Persona, Dayt));

            (await As(Nico).Post("/api/joinAsNewcomer", new { eventId = "e1", excludedPoolGameIds = new[] { "p1" } })).Is(HttpStatusCode.BadRequest);
            (await As(Nico).Post("/api/joinAsNewcomer", new { eventId = "e1", excludedPoolGameIds = new[] { "unknown" } })).Ok();
        }

        [Fact]
        public async Task Join_WhenEveryGameIsReceived_IsRefused()
        {
            await SeedEvent("e1", EventStatusId.Active, new[] { Sliva, Dayt }, new[] { Elden, Hollow }, allowsNewcomers: true);
            await Seed(Make.PoolGame("p1", "e1", Sekiro, Sliva), Make.Event("old", G1, Sliva, EventStatusId.Finished), Make.Record("old-r0", "old", Sliva, Nico, Sekiro));

            Assert.Equal("Newcomers can't join the event now",
                (await As(Nico).Post("/api/joinAsNewcomer", new { eventId = "e1", excludedPoolGameIds = Array.Empty<string>() })).Is(HttpStatusCode.BadRequest).Text);
        }

        [Fact]
        public async Task Pool_AGameThatDroppedToANewcomer_CantBeChanged()
        {
            // the event is back at the registration only through the database, the rule guards the dropped games anyway
            await SeedEvent("e1", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt }, allowsNewcomers: true);
            await Seed(Make.PoolGame("p1", "e1", Sekiro, Sliva), Make.Record("e1-new", "e1", Sliva, Nico, Sekiro, poolGameId: "p1"));

            Assert.Equal("The game has already dropped to a newcomer",
                (await As(Sliva).Post("/api/removeNewcomerPoolGame", new { eventId = "e1", poolGameId = "p1" })).Is(HttpStatusCode.Conflict).Text);
        }
    }
}
