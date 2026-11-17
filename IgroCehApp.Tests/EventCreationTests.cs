using System.Net;
using Domain.Enums;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // a regular event of a guild: its creation in a season, the list of the guild and the page of the event
    public class EventCreationTests : GuildTest
    {
        public EventCreationTests(ApiFactory factory) : base(factory)
        {
        }

        private static string Offset(DateTime date) => new DateTimeOffset(date, TimeSpan.Zero).ToString("yyyy-MM-ddTHH:mm:sszzz");

        private static DateTime Today => DateTime.UtcNow.Date;

        // the own dates of the advanced options are days
        private Task<ApiResult> CreateWithDates(DateTime? start, DateTime? end, bool? isNextSeason = null) =>
            As(Sliva).Post("/api/postNewEvent", new
            {
                eventName = "Own dates",
                guildId = G1,
                allowsNewcomers = false,
                isNextSeason,
                startDate = start?.ToString("yyyy-MM-dd"),
                endDate = end?.ToString("yyyy-MM-dd"),
            });

        [Fact]
        public async Task Seasons_AreTheCurrentAndTheNextOne()
        {
            var seasons = (await As(Sliva).Get("/api/getEventSeasons")).Ok().At("seasons");

            Assert.Equal(Offset(Seasons.CurrentStart()), seasons.Str("current.start"));
            Assert.Equal(Offset(Seasons.NextStart()), seasons.Str("current.end"));
            Assert.Equal(Offset(Seasons.NextStart()), seasons.Str("next.start"));
            Assert.Equal(Offset(Seasons.NextStart().AddMonths(3)), seasons.Str("next.end"));
            Assert.Equal(Seasons.IsLastMonth(), seasons.Bool("isNextByDefault"));
        }

        [Fact]
        public async Task Create_InTheCurrentSeason_TheCreatorIsTheFirstPlayer()
        {
            var eventId = await CreateEvent("Spring", isNextSeason: false);

            var page = await GetEvent(eventId);
            Assert.Equal(eventId, page.Str("id"));
            Assert.Equal("Spring", page.Str("eventName"));
            Assert.Equal(Sliva, page.Str("eventCreatorId"));
            Assert.Equal((int)EventStatusId.PlayersRegistration, page.Int("statusId"));
            Assert.False(page.Bool("allowsNewcomers"));
            Assert.Equal(Offset(Seasons.CurrentStart()), page.Str("seasonStartsAt"));
            Assert.True(page.Bool("isSeason"));
            var record = Assert.Single(page.Arr("eventRecords"));
            Assert.Equal(Sliva, record.Str("participant.id"));
            Assert.Equal("Sliva", record.Str("participant.userName"));
            Assert.Null(record.At("toUser"));
            Assert.Null(record.At("game"));
            Assert.Null(record.At("reward"));
            Assert.False(record.Bool("isNewcomer"));
        }

        [Fact]
        public async Task Create_InTheNextSeason()
        {
            var eventId = await CreateEvent(isNextSeason: true);

            Assert.Equal(Offset(Seasons.NextStart()), (await GetEvent(eventId)).Str("seasonStartsAt"));
            var saved = await Db(db => db.Events.SingleAsync(e => e.Id == eventId));
            Assert.Equal(Seasons.NextStart().AddMonths(3), saved.EndDate);
        }

        [Fact]
        public async Task Create_WithoutAChoice_TheLastMonthOfASeasonGivesTheNextOne()
        {
            var eventId = await CreateEvent(isNextSeason: null);

            var expected = Seasons.IsLastMonth() ? Seasons.NextStart() : Seasons.CurrentStart();
            Assert.Equal(Offset(expected), (await GetEvent(eventId)).Str("seasonStartsAt"));
        }

        // the front gets the bounds of the own dates as days and doesn't count them
        [Fact]
        public async Task PeriodLimits_GiveTheBoundsAsDays_ForTheStart()
        {
            static string Day(DateTime date) => date.ToString("yyyy-MM-dd");
            var start = Today.AddDays(3);

            var limits = (await As(Sliva).Get($"/api/getEventPeriodLimits?startDate={Day(start)}")).Ok().At("periodLimits");
            var fromToday = (await As(Sliva).Get("/api/getEventPeriodLimits")).Ok().At("periodLimits");

            Assert.Equal((Day(Today), Day(Today.AddMonths(12))), (limits.Str("earliestStart"), limits.Str("latestStart")));
            Assert.Equal((Day(start), Day(start.AddDays(14)), Day(start.AddMonths(6))),
                (limits.Str("startDate"), limits.Str("earliestEnd"), limits.Str("latestEnd")));
            Assert.Equal((14, 6, 12), (limits.Int("minDays"), limits.Int("maxMonths"), limits.Int("maxStartMonths")));
            Assert.Equal((Day(Today), Day(Today.AddDays(14))), (fromToday.Str("startDate"), fromToday.Str("earliestEnd")));
            (await Anonymous.Get("/api/getEventPeriodLimits")).Is(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Create_WithOwnDates_InsteadOfTheSeason()
        {
            var start = Today.AddDays(3);
            var end = start.AddDays(30);

            // the switch of the season doesn't count with own dates
            var eventId = (await CreateWithDates(start, end, isNextSeason: true)).Ok().Str("eventId")!;

            var saved = await Db(db => db.Events.SingleAsync(e => e.Id == eventId));
            Assert.Equal((start, end), (saved.StartDate, saved.EndDate));
            var page = await GetEvent(eventId);
            Assert.Equal(Offset(start), page.Str("seasonStartsAt"));
            Assert.False(page.Bool("isSeason"));
            var tile = Assert.Single((await As(Sliva).Get($"/api/getEventsByGuildId?guildId={G1}&startFrom=0")).Ok().Arr("eventsList"));
            Assert.Equal((start, end), (DateTime.Parse(tile.Str("startDate")!), DateTime.Parse(tile.Str("endDate")!)));
        }

        [Fact]
        public async Task Create_WithOwnDatesOfASeason_IsASeason()
        {
            var eventId = (await CreateWithDates(Seasons.NextStart(), Seasons.NextStart().AddMonths(3))).Ok().Str("eventId")!;

            Assert.True((await GetEvent(eventId)).Bool("isSeason"));
        }

        [Fact]
        public async Task Create_WithOwnDates_KeepsThemInTheLimits()
        {
            async Task Refused(DateTime? start, DateTime? end, string message)
            {
                var answer = await CreateWithDates(start, end);
                answer.Is(HttpStatusCode.BadRequest);
                Assert.Contains(message, answer.Body);
            }

            await Refused(Today.AddDays(3), null, "An event with its own dates needs both the start and the end");
            await Refused(null, Today.AddDays(30), "An event with its own dates needs both the start and the end");
            await Refused(Today.AddDays(-2), Today.AddDays(30), "The event can't start in the past");
            await Refused(Today.AddMonths(12).AddDays(1), Today.AddMonths(13), "The start can be planned up to 12 months ahead");
            await Refused(Today.AddDays(3), Today.AddDays(3 + 13), "An event lasts at least 14 days");
            await Refused(Today.AddDays(3), Today.AddDays(3).AddMonths(6).AddDays(1), "An event lasts up to 6 months");
            await Refused(Today.AddDays(30), Today.AddDays(3), "An event lasts at least 14 days");
            Assert.Equal(0, await Factory.ScalarAsync("SELECT COUNT(*) FROM Events"));

            // the bounds themselves; yesterday is allowed for the time zones behind UTC
            (await CreateWithDates(Today.AddDays(-1), Today.AddDays(13))).Ok();
            (await CreateWithDates(Today.AddMonths(12), Today.AddMonths(12).AddDays(14))).Ok();
            (await CreateWithDates(Today.AddDays(3), Today.AddDays(3).AddMonths(6))).Ok();
            Assert.Equal(3, await Factory.ScalarAsync("SELECT COUNT(*) FROM Events"));
        }

        [Fact]
        public async Task Create_WithNewcomers()
        {
            var eventId = await CreateEvent(allowsNewcomers: true);

            Assert.True((await GetEvent(eventId)).Bool("allowsNewcomers"));
        }

        [Fact]
        public async Task Create_InAGuildOfOthers_IsRefused()
        {
            var answer = await As(Stranger).Post("/api/postNewEvent", new { eventName = "Mine", guildId = G1, isNextSeason = false });

            answer.Is(HttpStatusCode.BadRequest);
            Assert.Equal("Couldn't create new event", answer.Text);
            Assert.Equal(0, await Factory.ScalarAsync("SELECT COUNT(*) FROM Events"));
        }

        [Fact]
        public async Task Create_ByAPlayerWhoLeftTheGuild_IsAllowed()
        {
            // the membership is checked without IsDeleted (current behavior)
            var eventId = await CreateEvent(creator: Leaver);

            Assert.NotNull(eventId);
        }

        [Fact]
        public async Task Create_SeveralUnfinishedEventsOfOneCreator_AreAllowed()
        {
            await CreateEvent("First");
            await CreateEvent("Second");

            Assert.Equal(2, await Factory.ScalarAsync("SELECT COUNT(*) FROM Events"));
        }

        [Fact]
        public async Task Page_IsSeenByMembersOfTheGuildOnly()
        {
            var eventId = await CreateEvent();

            (await As(Dayt).Get($"/api/getEventById?eventId={eventId}")).Is(HttpStatusCode.OK);
            (await As(Stranger).Get($"/api/getEventById?eventId={eventId}")).Is(HttpStatusCode.NotFound);
            (await As(Sliva).Get("/api/getEventById?eventId=unknown")).Is(HttpStatusCode.NotFound);
            // the membership is checked without IsDeleted (current behavior)
            (await As(Leaver).Get($"/api/getEventById?eventId={eventId}")).Is(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Page_ShowsTheChainOfGamesAndTheNewcomers()
        {
            await SeedEvent("e1", EventStatusId.Active, new[] { Sliva, Dayt }, new[] { Elden, Hollow });
            await Seed(Make.PoolGame("p1", "e1", Sekiro, Sliva), Make.Record("e1-new", "e1", Sliva, Nico, Sekiro, poolGameId: "p1"));

            var records = (await GetEvent("e1")).Arr("eventRecords").OrderBy(r => r.Str("id")).ToList();

            Assert.Equal(new[] { "e1-new", "e1-r0", "e1-r1" }, records.Select(r => r.Str("id")));
            Assert.Equal((Sliva, Nico, "Sekiro", true), (records[0].Str("participant.id"), records[0].Str("toUser.id"), records[0].Str("game.name"), records[0].Bool("isNewcomer")));
            Assert.Equal((Sliva, Dayt, "Elden Ring", false), (records[1].Str("participant.id"), records[1].Str("toUser.id"), records[1].Str("game.name"), records[1].Bool("isNewcomer")));
            Assert.Equal((Dayt, Sliva, "Hollow Knight"), (records[2].Str("participant.id"), records[2].Str("toUser.id"), records[2].Str("game.name")));
        }

        [Fact]
        public async Task List_OfTheGuild_GivesTheTilesOfTheEvents()
        {
            var eventId = await CreateEvent("Tile", allowsNewcomers: true);
            (await As(Dayt).Post("/api/joinEvent", new { eventId })).Ok();

            var tile = Assert.Single((await As(Nico).Get($"/api/getEventsByGuildId?guildId={G1}&startFrom=0")).Ok().Arr("eventsList"));

            Assert.Equal(eventId, tile.Str("id"));
            Assert.Equal("Tile", tile.Str("name"));
            Assert.Equal(Seasons.CurrentStart(), DateTime.Parse(tile.Str("startDate")!));
            Assert.Equal(Seasons.NextStart(), DateTime.Parse(tile.Str("endDate")!));
            Assert.Equal(Sliva, tile.Str("creatorId"));
            Assert.Equal("Sliva", tile.Str("creatorUserName"));
            Assert.Equal(G1, tile.Str("guildId"));
            Assert.Equal("Guild One", tile.Str("guildName"));
            Assert.Equal(0, tile.Int("statusId"));
            Assert.Equal("Players registration", tile.Str("statusDisplayName"));
            Assert.Equal(new[] { Dayt, Sliva }, tile.Strs("participantsIds").OrderBy(id => id));
            Assert.True(tile.Bool("allowsNewcomers"));
        }

        [Fact]
        public async Task List_OfAGuildOfOthers_IsEmpty()
        {
            await CreateEvent();

            Assert.Empty((await As(Stranger).Get($"/api/getEventsByGuildId?guildId={G1}&startFrom=0")).Ok().Arr("eventsList"));
        }

        [Fact]
        public async Task List_ActiveFirst_ThenEarlierStages_FinishedLast_NewerSeasonsFirst()
        {
            var season = Seasons.CurrentStart();
            await Seed(
                Make.Event("finished", G1, Sliva, EventStatusId.Finished, season.AddMonths(3)),
                Make.Event("registration", G1, Sliva, EventStatusId.PlayersRegistration, season),
                Make.Event("shuffle", G1, Sliva, EventStatusId.PlayersShuffle, season),
                Make.Event("guessing", G1, Sliva, EventStatusId.GamesGuessing, season),
                Make.Event("active-old", G1, Sliva, EventStatusId.Active, season.AddMonths(-3)),
                Make.Event("active-new", G1, Sliva, EventStatusId.Active, season),
                Make.Event("deleted", G1, Sliva, EventStatusId.Active, season, isDeleted: true),
                Make.Event("other-guild", G2, Stranger, EventStatusId.Active, season));

            var list = (await As(Sliva).Get($"/api/getEventsByGuildId?guildId={G1}&startFrom=0")).Ok();

            Assert.Equal(new[] { "active-new", "active-old", "guessing", "shuffle", "registration", "finished" }, list.Strs("eventsList", "id"));
        }

        [Fact]
        public async Task List_IsGivenByTenEvents()
        {
            var season = Seasons.CurrentStart();
            await Seed(Enumerable.Range(0, 12).Select(i => (object)Make.Event($"e{i:00}", G1, Sliva, EventStatusId.Finished, season.AddMonths(-3 * i))).ToArray());

            var first = (await As(Sliva).Get($"/api/getEventsByGuildId?guildId={G1}&startFrom=0")).Ok().Strs("eventsList", "id");
            var second = (await As(Sliva).Get($"/api/getEventsByGuildId?guildId={G1}&startFrom=10")).Ok().Strs("eventsList", "id");

            Assert.Equal(Enumerable.Range(0, 10).Select(i => $"e{i:00}"), first);
            Assert.Equal(new[] { "e10", "e11" }, second);
        }
    }
}
