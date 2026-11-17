using System.Net;
using Domain.Enums;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // the stages the creator moves an event through: registration, shuffle, guessing; the players join and leave during the registration
    public class EventStageTests : GuildTest
    {
        public EventStageTests(ApiFactory factory) : base(factory)
        {
        }

        private Task<ApiResult> Move(string eventId, EventStatusId to, string by = Sliva) =>
            As(by).Post("/api/moveEventToNextStage", new { eventId, statusId = (int)to });

        private async Task<EventStatusId> StatusOf(string eventId) =>
            await Db(db => db.Events.IgnoreQueryFilters().Where(e => e.Id == eventId).Select(e => e.StatusId).SingleAsync());

        [Fact]
        public async Task Join_AMemberJoinsOnce()
        {
            var eventId = await CreateEvent();

            var joined = (await As(Dayt).Post("/api/joinEvent", new { eventId })).Ok();
            var again = await As(Dayt).Post("/api/joinEvent", new { eventId });

            Assert.Equal(Dayt, joined.Str("eventRecordObject.participant.id"));
            Assert.NotNull(joined.Str("eventRecordObject.id"));
            again.Is(HttpStatusCode.Conflict);
            Assert.Equal("You have already joined the event", again.Text);
            Assert.Equal(2, (await GetEvent(eventId)).Arr("eventRecords").Count);
        }

        [Fact]
        public async Task Join_IsRefused_ToStrangers_LeftPlayers_AndUnknownEvents()
        {
            var eventId = await CreateEvent();

            (await As(Stranger).Post("/api/joinEvent", new { eventId })).Is(HttpStatusCode.Forbidden);
            (await As(Leaver).Post("/api/joinEvent", new { eventId })).Is(HttpStatusCode.Forbidden);
            (await As(Dayt).Post("/api/joinEvent", new { eventId = "unknown" })).Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Join_AfterTheRegistration_IsRefused()
        {
            await SeedEvent("e1", EventStatusId.PlayersShuffle, new[] { Sliva, Dayt });

            var answer = await As(Nico).Post("/api/joinEvent", new { eventId = "e1" });

            answer.Is(HttpStatusCode.BadRequest);
            Assert.Equal("The event doesn't gather players any more", answer.Text);
        }

        [Fact]
        public async Task Join_ADeletedEvent_IsNotFound()
        {
            await Seed(Make.Event("e1", G1, Sliva, isDeleted: true));

            (await As(Dayt).Post("/api/joinEvent", new { eventId = "e1" })).Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Leave_APlayerLeavesByThemselves_TheCreatorRemovesAnyone_OthersCant()
        {
            var eventId = await CreateEvent();
            (await As(Dayt).Post("/api/joinEvent", new { eventId })).Ok();
            (await As(Nico).Post("/api/joinEvent", new { eventId })).Ok();

            (await As(Dayt).Post("/api/removeFromEvent", new { eventId, userId = Nico })).Is(HttpStatusCode.Forbidden);
            var left = (await As(Dayt).Post("/api/removeFromEvent", new { eventId, userId = Dayt })).Ok();
            (await As(Sliva).Post("/api/removeFromEvent", new { eventId, userId = Nico })).Ok();

            Assert.Equal(Dayt, left.Str("eventRecordObject.participant.id"));
            Assert.Equal(new[] { Sliva }, (await GetEvent(eventId)).Strs("eventRecords", "participant.id"));
        }

        [Fact]
        public async Task Leave_TheCreatorMayLeaveTheirOwnEvent()
        {
            var eventId = await CreateEvent();

            (await As(Sliva).Post("/api/removeFromEvent", new { eventId, userId = Sliva })).Ok();

            Assert.Empty((await GetEvent(eventId)).Arr("eventRecords"));
        }

        [Fact]
        public async Task Leave_SomeoneNotInTheEvent_IsNotFound()
        {
            var eventId = await CreateEvent();

            (await As(Sliva).Post("/api/removeFromEvent", new { eventId, userId = Nico })).Is(HttpStatusCode.NotFound);
            (await As(Sliva).Post("/api/removeFromEvent", new { eventId = "unknown", userId = Sliva })).Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Leave_AfterTheRegistration_IsRefused()
        {
            await SeedEvent("e1", EventStatusId.PlayersShuffle, new[] { Sliva, Dayt });

            var answer = await As(Dayt).Post("/api/removeFromEvent", new { eventId = "e1", userId = Dayt });

            answer.Is(HttpStatusCode.BadRequest);
            Assert.Equal("Players leave the event only during the registration", answer.Text);
        }

        [Fact]
        public async Task Leave_TakesTheGamesOfThePlayerOutOfThePool()
        {
            await SeedEvent("e1", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt }, allowsNewcomers: true);
            await Seed(Make.PoolGame("p1", "e1", Elden, Sliva), Make.PoolGame("p2", "e1", Hollow, Dayt));

            (await As(Dayt).Post("/api/removeFromEvent", new { eventId = "e1", userId = Dayt })).Ok();

            Assert.Equal(new[] { "p1" }, await Db(db => db.NewcomerPoolGames.Select(pg => pg.Id).ToListAsync()));
        }

        [Fact]
        public async Task Move_ToTheShuffle_NeedsTwoPlayers()
        {
            var eventId = await CreateEvent();

            var alone = await Move(eventId, EventStatusId.PlayersShuffle);
            (await As(Dayt).Post("/api/joinEvent", new { eventId })).Ok();
            var moved = (await Move(eventId, EventStatusId.PlayersShuffle)).Ok();

            alone.Is(HttpStatusCode.BadRequest);
            Assert.Equal("The event can't move to this stage now", alone.Text);
            Assert.Equal(1, moved.Int("moveToStage"));
            Assert.Equal(EventStatusId.PlayersShuffle, await StatusOf(eventId));
        }

        [Fact]
        public async Task Move_OfAnEventOfTheNextSeason_IsntHeldBySeason()
        {
            var eventId = await CreateEvent(isNextSeason: true);
            (await As(Dayt).Post("/api/joinEvent", new { eventId })).Ok();

            (await Move(eventId, EventStatusId.PlayersShuffle)).Ok();
        }

        [Fact]
        public async Task Move_WithNewcomers_NeedsThreeGamesInThePool()
        {
            await SeedEvent("e1", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt, Nico }, allowsNewcomers: true);
            await Seed(Make.PoolGame("p1", "e1", Elden, Sliva), Make.PoolGame("p2", "e1", Hollow, Dayt));

            (await Move("e1", EventStatusId.PlayersShuffle)).Is(HttpStatusCode.BadRequest);
            await Seed(Make.PoolGame("p3", "e1", Sekiro, Nico));
            (await Move("e1", EventStatusId.PlayersShuffle)).Ok();
        }

        [Fact]
        public async Task Move_OnlyByTheCreator_OnlyToTheNextStage()
        {
            var eventId = await CreateEvent();
            (await As(Dayt).Post("/api/joinEvent", new { eventId })).Ok();

            (await Move(eventId, EventStatusId.PlayersShuffle, by: Dayt)).Is(HttpStatusCode.Forbidden);
            (await Move(eventId, EventStatusId.PlayersShuffle, by: Admin)).Is(HttpStatusCode.Forbidden);
            (await Move(eventId, EventStatusId.GamesGuessing)).Is(HttpStatusCode.BadRequest);
            (await Move(eventId, EventStatusId.Active)).Is(HttpStatusCode.BadRequest);
            (await Move("unknown", EventStatusId.PlayersShuffle)).Is(HttpStatusCode.NotFound);
            // a repeated request changes nothing
            (await Move(eventId, EventStatusId.PlayersShuffle)).Ok();
            (await Move(eventId, EventStatusId.PlayersShuffle)).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Move_ToGuessing_NeedsTheShuffle()
        {
            await SeedEvent("e1", EventStatusId.PlayersShuffle, new[] { Sliva, Dayt, Nico });

            (await Move("e1", EventStatusId.GamesGuessing)).Is(HttpStatusCode.BadRequest);
            (await As(Sliva).Post("/api/shuffleUsers", new { eventId = "e1" })).Ok();
            (await Move("e1", EventStatusId.GamesGuessing)).Ok();
        }

        [Fact]
        public async Task Move_FromGuessing_IsNotManual()
        {
            await SeedEvent("e1", EventStatusId.GamesGuessing, new[] { Sliva, Dayt }, new[] { Elden, Hollow });

            (await Move("e1", EventStatusId.RevealingGames)).Is(HttpStatusCode.BadRequest);
            (await Move("e1", EventStatusId.Active)).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Shuffle_MakesOneCircleOfAllPlayers()
        {
            var players = new[] { Sliva, Dayt, Nico, Admin };
            await SeedEvent("e1", EventStatusId.PlayersShuffle, players);

            var records = (await As(Sliva).Post("/api/shuffleUsers", new { eventId = "e1" })).Ok().Arr("eventRecordObjects");

            var next = records.ToDictionary(r => r.Str("participant.id")!, r => r.Str("toUser.id")!);
            Assert.Equal(players.OrderBy(p => p), next.Keys.OrderBy(p => p));
            Assert.Equal(players.OrderBy(p => p), next.Values.OrderBy(p => p));
            Assert.All(next, pair => Assert.NotEqual(pair.Key, pair.Value));
            // walking from anyone visits everyone before coming back
            var visited = new List<string> { Sliva };
            while (next[visited[^1]] != Sliva)
            {
                visited.Add(next[visited[^1]]);
            }

            Assert.Equal(players.Length, visited.Count);
            var saved = await Db(db => db.EventRecords.Where(er => er.EventId == "e1").ToDictionaryAsync(er => er.ParticipantId, er => er.ToUserId));
            Assert.Equal(next.OrderBy(p => p.Key), saved.OrderBy(p => p.Key)!);
        }

        [Fact]
        public async Task Shuffle_CanBeRepeatedOnItsStage()
        {
            await SeedEvent("e1", EventStatusId.PlayersShuffle, new[] { Sliva, Dayt });

            (await As(Sliva).Post("/api/shuffleUsers", new { eventId = "e1" })).Ok();
            (await As(Sliva).Post("/api/shuffleUsers", new { eventId = "e1" })).Ok();
        }

        [Fact]
        public async Task Shuffle_OnlyByTheCreator_OnlyOnItsStage()
        {
            await SeedEvent("e1", EventStatusId.PlayersShuffle, new[] { Sliva, Dayt });
            await SeedEvent("e2", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt });
            await SeedEvent("e3", EventStatusId.GamesGuessing, new[] { Sliva, Dayt });

            var byOther = await As(Dayt).Post("/api/shuffleUsers", new { eventId = "e1" });

            byOther.Is(HttpStatusCode.BadRequest);
            Assert.Equal("", byOther.Body);
            (await As(Sliva).Post("/api/shuffleUsers", new { eventId = "e2" })).Is(HttpStatusCode.BadRequest);
            (await As(Sliva).Post("/api/shuffleUsers", new { eventId = "e3" })).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Guess_APlayerGuessesForTheOneTheyGot_AndCanChangeIt()
        {
            await SeedEvent("e1", EventStatusId.GamesGuessing, new[] { Sliva, Dayt });

            var guessed = (await As(Sliva).Post("/api/suggestGame", new { eventId = "e1", eventRecordId = "e1-r0", gameId = Elden })).Ok();
            var changed = (await As(Sliva).Post("/api/suggestGame", new { eventId = "e1", eventRecordId = "e1-r0", gameId = Hollow })).Ok();

            Assert.Equal(("e1-r0", "Elden Ring", Dayt), (guessed.Str("id"), guessed.Str("game.name"), guessed.Str("toUser.id")));
            Assert.Equal(Hollow, changed.Str("game.id"));
            Assert.Equal(Hollow, await Db(db => db.EventRecords.Where(er => er.Id == "e1-r0").Select(er => er.GameId).SingleAsync()));
        }

        [Fact]
        public async Task Guess_IsRefused_ToOthers_OutsideItsStage_AndForUnknownGames()
        {
            await SeedEvent("e1", EventStatusId.GamesGuessing, new[] { Sliva, Dayt });
            await SeedEvent("e2", EventStatusId.Active, new[] { Sliva, Dayt }, new[] { Elden, Hollow });

            (await As(Dayt).Post("/api/suggestGame", new { eventId = "e1", eventRecordId = "e1-r0", gameId = Elden })).Is(HttpStatusCode.Forbidden);
            (await As(Sliva).Post("/api/suggestGame", new { eventId = "e1", eventRecordId = "e1-r0", gameId = "unknown" })).Is(HttpStatusCode.NotFound);
            (await As(Sliva).Post("/api/suggestGame", new { eventId = "e1", eventRecordId = "unknown", gameId = Elden })).Is(HttpStatusCode.NotFound);
            var late = await As(Sliva).Post("/api/suggestGame", new { eventId = "e2", eventRecordId = "e2-r0", gameId = Sekiro });
            late.Is(HttpStatusCode.BadRequest);
            Assert.Equal("The game can't be guessed now", late.Text);
        }

        [Fact]
        public async Task Guess_AGameThePlayerHasGotBefore_IsRefused_UnlessThatEventIsDeleted()
        {
            await SeedEvent("old", EventStatusId.Finished, new[] { Nico, Dayt }, new[] { Elden, Hollow }, start: Seasons.CurrentStart().AddMonths(-3));
            await Seed(Make.Event("deleted", G1, Nico, EventStatusId.Finished, isDeleted: true), Make.Record("deleted-r0", "deleted", Nico, Dayt, Sekiro));
            await SeedEvent("e1", EventStatusId.GamesGuessing, new[] { Sliva, Dayt });

            var repeated = await As(Sliva).Post("/api/suggestGame", new { eventId = "e1", eventRecordId = "e1-r0", gameId = Elden });

            repeated.Is(HttpStatusCode.Conflict);
            Assert.Equal("The player has already got this game", repeated.Text);
            (await As(Sliva).Post("/api/suggestGame", new { eventId = "e1", eventRecordId = "e1-r0", gameId = Sekiro })).Ok();
        }
    }
}
