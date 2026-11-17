using System.Net;
using Application.Services;
using Domain.Enums;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // an event after guessing: the activation, the passing of the games with their rewards, summarizing and deleting
    public class EventPlayTests : GuildTest
    {
        public EventPlayTests(ApiFactory factory) : base(factory)
        {
        }

        private async Task<EventStatusId> StatusOf(string eventId) =>
            await Db(db => db.Events.IgnoreQueryFilters().Where(e => e.Id == eventId).Select(e => e.StatusId).SingleAsync());

        [Fact]
        public async Task Activate_WhenEveryGameIsGuessed_AndTheSeasonHasStarted()
        {
            await SeedEvent("e1", EventStatusId.GamesGuessing, new[] { Sliva, Dayt }, new[] { Elden, Hollow });

            (await As(Sliva).Post("/api/activateEvent", new { eventId = "e1" })).Ok();

            Assert.Equal(EventStatusId.Active, await StatusOf("e1"));
        }

        [Fact]
        public async Task Activate_BeforeEveryGameIsGuessed_IsRefused()
        {
            await SeedEvent("e1", EventStatusId.GamesGuessing, new[] { Sliva, Dayt }, new[] { Elden });

            var answer = await As(Sliva).Post("/api/activateEvent", new { eventId = "e1" });

            answer.Is(HttpStatusCode.BadRequest);
            Assert.Equal("The event can be activated only when every participant has guessed a game", answer.Text);
        }

        [Fact]
        public async Task Activate_BeforeTheSeason_IsRefused_AfterTheGuesses()
        {
            await SeedEvent("e1", EventStatusId.GamesGuessing, new[] { Sliva, Dayt }, new[] { Elden }, start: Seasons.NextStart());
            await SeedEvent("e2", EventStatusId.GamesGuessing, new[] { Sliva, Dayt }, new[] { Elden, Hollow }, start: Seasons.NextStart());

            Assert.Equal("The event can be activated only when every participant has guessed a game",
                (await As(Sliva).Post("/api/activateEvent", new { eventId = "e1" })).Is(HttpStatusCode.BadRequest).Text);
            Assert.Equal("The season of the event hasn't started yet",
                (await As(Sliva).Post("/api/activateEvent", new { eventId = "e2" })).Is(HttpStatusCode.BadRequest).Text);
            Assert.Equal(EventStatusId.GamesGuessing, await StatusOf("e2"));
        }

        [Fact]
        public async Task Activate_OnlyByTheCreator_OnlyAfterGuessing()
        {
            await SeedEvent("e1", EventStatusId.GamesGuessing, new[] { Sliva, Dayt }, new[] { Elden, Hollow });
            await SeedEvent("e2", EventStatusId.PlayersShuffle, new[] { Sliva, Dayt }, new[] { Elden, Hollow });

            (await As(Dayt).Post("/api/activateEvent", new { eventId = "e1" })).Is(HttpStatusCode.Forbidden);
            (await As(Admin).Post("/api/activateEvent", new { eventId = "e1" })).Is(HttpStatusCode.Forbidden);
            (await As(Sliva).Post("/api/activateEvent", new { eventId = "unknown" })).Is(HttpStatusCode.NotFound);
            (await As(Sliva).Post("/api/activateEvent", new { eventId = "e2" })).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task WholeEvent_FromCreationToTheScores()
        {
            var eventId = await CreateEvent("Whole");
            (await As(Dayt).Post("/api/joinEvent", new { eventId })).Ok();
            (await As(Nico).Post("/api/joinEvent", new { eventId })).Ok();
            (await As(Sliva).Post("/api/moveEventToNextStage", new { eventId, statusId = 1 })).Ok();
            var records = (await As(Sliva).Post("/api/shuffleUsers", new { eventId })).Ok().Arr("eventRecordObjects");
            (await As(Sliva).Post("/api/moveEventToNextStage", new { eventId, statusId = 2 })).Ok();
            var games = new Queue<string>(new[] { Elden, Hollow, Sekiro });
            foreach (var record in records)
            {
                (await As(record.Str("participant.id")!).Post("/api/suggestGame", new { eventId, eventRecordId = record.Str("id"), gameId = games.Dequeue() })).Ok();
            }

            (await As(Sliva).Post("/api/activateEvent", new { eventId })).Ok();
            var daytRecord = records.Single(r => r.Str("toUser.id") == Dayt);
            var passed = (await As(daytRecord.Str("participant.id")!).Post("/api/submitPassing", new { eventId, eventRecordId = daytRecord.Str("id") })).Ok();
            (await As(Sliva).Post("/api/summarizeEvent", new { eventId })).Ok();

            // the months are counted from the start of the season of the event, not from its activation
            var reward = EventApplicationService.CalculatePassingReward(new DateTimeOffset(Seasons.CurrentStart(), TimeSpan.Zero), DateTimeOffset.UtcNow);
            Assert.Equal(reward, passed.Int("eventRecordObject.reward"));
            Assert.Equal(EventStatusId.Finished, await StatusOf(eventId));
            var scores = (await As(Sliva).Get($"/api/getScoresByGuild?guildId={G1}")).Ok().Arr("scores");
            Assert.Equal(reward, scores.Single(s => s.Str("userId") == Dayt).Int("score"));
            Assert.Equal(0, scores.Single(s => s.Str("userId") == Nico).Int("score"));
        }

        [Theory]
        [InlineData(0, 10, 3)]
        [InlineData(1, 2, 2)]
        [InlineData(2, 2, 1)]
        [InlineData(3, 2, 0)]
        [InlineData(5, 0, 0)]
        public async Task Passing_TheRewardFallsWithTheMonthsSinceTheSeasonStart(int months, int days, int reward)
        {
            // the season of the event is shifted, so the passing comes after the given months and days of it
            await SeedEvent("e1", EventStatusId.Active, new[] { Sliva, Dayt }, new[] { Elden, Hollow },
                start: DateTime.UtcNow.AddMonths(-months).AddDays(-days));

            var answer = (await As(Sliva).Post("/api/submitPassing", new { eventId = "e1", eventRecordId = "e1-r0" })).Ok();

            Assert.Equal(reward, answer.Int("eventRecordObject.reward"));
            var saved = await Db(db => db.EventRecords.SingleAsync(er => er.Id == "e1-r0"));
            Assert.Equal(reward, saved.Reward);
            Assert.True(saved.SucceededAt > DateTimeOffset.UtcNow.AddMinutes(-1));
        }

        [Theory]
        [InlineData("2026-03-01T00:00:00Z", "2026-03-01T00:00:00Z", 3)]
        [InlineData("2026-03-01T00:00:00Z", "2026-03-31T23:59:59Z", 3)]
        [InlineData("2026-03-01T00:00:00Z", "2026-04-01T00:00:00Z", 2)]
        [InlineData("2026-03-01T00:00:00Z", "2026-05-01T00:00:00Z", 1)]
        [InlineData("2026-03-01T00:00:00Z", "2026-05-31T23:59:59Z", 1)]
        [InlineData("2026-03-01T00:00:00Z", "2026-06-01T00:00:00Z", 0)]
        [InlineData("2026-03-15T00:00:00Z", "2026-04-14T23:59:59Z", 3)]
        [InlineData("2026-03-15T00:00:00Z", "2026-04-15T00:00:00Z", 2)]
        [InlineData("2026-03-01T00:00:00Z", "2026-02-20T00:00:00Z", 3)]
        public void PassingReward_CountsFullMonths(string countFrom, string passedAt, int reward)
        {
            Assert.Equal(reward, EventApplicationService.CalculatePassingReward(DateTimeOffset.Parse(countFrom), DateTimeOffset.Parse(passedAt)));
        }

        [Fact]
        public async Task Passing_IsSubmittedOnce_ByTheOneWhoGuessed_OrAnAdminOfTheGuild()
        {
            await SeedEvent("e1", EventStatusId.Active, new[] { Sliva, Dayt, Nico }, new[] { Elden, Hollow, Sekiro });

            (await As(Dayt).Post("/api/submitPassing", new { eventId = "e1", eventRecordId = "e1-r0" })).Is(HttpStatusCode.Forbidden);
            (await As(Leaver).Post("/api/submitPassing", new { eventId = "e1", eventRecordId = "e1-r0" })).Is(HttpStatusCode.Forbidden);
            (await As(Sliva).Post("/api/submitPassing", new { eventId = "e1", eventRecordId = "e1-r0" })).Ok();
            (await As(Admin).Post("/api/submitPassing", new { eventId = "e1", eventRecordId = "e1-r1" })).Ok();
            var again = await As(Sliva).Post("/api/submitPassing", new { eventId = "e1", eventRecordId = "e1-r0" });
            (await As(Sliva).Post("/api/submitPassing", new { eventId = "e1", eventRecordId = "unknown" })).Is(HttpStatusCode.NotFound);

            again.Is(HttpStatusCode.BadRequest);
            Assert.Equal("The game can't be marked as passed now", again.Text);
        }

        [Fact]
        public async Task Passing_BeforeTheActivation_IsRefused()
        {
            await SeedEvent("e1", EventStatusId.GamesGuessing, new[] { Sliva, Dayt }, new[] { Elden, Hollow });

            (await As(Sliva).Post("/api/submitPassing", new { eventId = "e1", eventRecordId = "e1-r0" })).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Summarize_ByTheCreatorOrAnAdmin_AtAnyStage()
        {
            await SeedEvent("e1", EventStatusId.PlayersRegistration, new[] { Sliva, Dayt });
            await SeedEvent("e2", EventStatusId.Active, new[] { Dayt, Nico }, new[] { Elden, Hollow });

            (await As(Sliva).Post("/api/summarizeEvent", new { eventId = "e1" })).Ok();
            (await As(Admin).Post("/api/summarizeEvent", new { eventId = "e2" })).Ok();

            Assert.Equal(EventStatusId.Finished, await StatusOf("e1"));
            Assert.Equal(EventStatusId.Finished, await StatusOf("e2"));
        }

        [Fact]
        public async Task Summarize_BySomeoneElse_IsSilentlySkipped()
        {
            await SeedEvent("e1", EventStatusId.Active, new[] { Sliva, Dayt }, new[] { Elden, Hollow });

            (await As(Dayt).Post("/api/summarizeEvent", new { eventId = "e1" })).Ok();
            (await As(Leaver).Post("/api/summarizeEvent", new { eventId = "e1" })).Ok();
            (await As(Dayt).Post("/api/summarizeEvent", new { eventId = "unknown" })).Ok();

            Assert.Equal(EventStatusId.Active, await StatusOf("e1"));
        }

        [Fact]
        public async Task Delete_ByAnAdminOfTheGuild_HidesTheEventAndItsRewards()
        {
            await SeedEvent("e1", EventStatusId.Active, new[] { Sliva, Dayt }, new[] { Elden, Hollow });
            (await As(Sliva).Post("/api/submitPassing", new { eventId = "e1", eventRecordId = "e1-r0" })).Ok();

            (await As(Admin).Post("/api/deleteEvent", new { eventId = "e1" })).Ok();

            (await As(Sliva).Get("/api/getEventById?eventId=e1")).Is(HttpStatusCode.NotFound);
            Assert.Empty((await As(Sliva).Get($"/api/getEventsByGuildId?guildId={G1}&startFrom=0")).Ok().Arr("eventsList"));
            var scores = (await As(Sliva).Get($"/api/getScoresByGuild?guildId={G1}")).Ok().Arr("scores");
            Assert.All(scores, s => Assert.Equal(0, s.Int("score")));
            var saved = await Db(db => db.Events.IgnoreQueryFilters().SingleAsync(e => e.Id == "e1"));
            Assert.True(saved.IsDeleted);
            Assert.Equal(Admin, saved.DeletedById);
            Assert.NotNull(saved.DeletedAt);
        }

        [Fact]
        public async Task Delete_IsOnlyForAdminsWhoStayInTheGuild()
        {
            await SeedEvent("e1", EventStatusId.Active, new[] { Sliva, Dayt }, new[] { Elden, Hollow });

            (await As(Sliva).Post("/api/deleteEvent", new { eventId = "e1" })).Is(HttpStatusCode.Forbidden);
            (await As(Leaver).Post("/api/deleteEvent", new { eventId = "e1" })).Is(HttpStatusCode.Forbidden);
            (await As(Admin).Post("/api/deleteEvent", new { eventId = "unknown" })).Is(HttpStatusCode.NotFound);
            (await As(Admin).Post("/api/deleteEvent", new { eventId = "e1" })).Ok();
            (await As(Admin).Post("/api/deleteEvent", new { eventId = "e1" })).Is(HttpStatusCode.NotFound);
        }
    }
}
