using System.Net;
using Domain.Enums;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // a paired review: the player who passed the game writes it, the one who guessed the game answers its sections
    public class ReviewTests : GuildTest
    {
        public ReviewTests(ApiFactory factory) : base(factory)
        {
        }

        // in e1 Sliva guessed Elden Ring for Dayt and Dayt passed it, Dayt guessed Hollow Knight for Sliva and Sliva hasn't passed it
        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await Seed(Make.Event("e1", G1, Sliva, EventStatusId.Active, name: "Spring"),
                Make.Record("e1-r0", "e1", Sliva, Dayt, Elden, 3, DateTimeOffset.UtcNow.AddDays(-2)),
                Make.Record("e1-r1", "e1", Dayt, Sliva, Hollow));
        }

        private static object Review(decimal? score = 8m, int? visibility = 2, params (string Title, string Text)[] sections) => new
        {
            eventRecordId = "e1-r0",
            score,
            visibility,
            sections = (sections.Length == 0 ? new[] { (Title: "Story", Text: "Good") } : sections).Select(s => new { title = s.Title, text = s.Text }).ToArray(),
        };

        [Fact]
        public async Task Page_BeforeTheReview()
        {
            var page = (await As(Nico).Get("/api/getReview?eventRecordId=e1-r0")).Ok();

            Assert.Equal(("e1-r0", "e1", "Spring", G1), (page.Str("eventRecordId"), page.Str("eventId"), page.Str("eventName"), page.Str("guildId")));
            Assert.Equal((Elden, "Elden Ring"), (page.Str("game.id"), page.Str("game.name")));
            Assert.Equal((Dayt, Sliva), (page.Str("author.id"), page.Str("guesser.id")));
            Assert.True(page.Bool("isPassed"));
            Assert.Null(page.At("review"));
            Assert.False((await As(Dayt).Get("/api/getReview?eventRecordId=e1-r1")).Ok().Bool("isPassed"));
        }

        [Fact]
        public async Task Page_IsHiddenFromStrangers_AndWithoutAGameOrAPlayer()
        {
            await Seed(Make.Record("e1-r2", "e1", Nico));

            (await As(Stranger).Get("/api/getReview?eventRecordId=e1-r0")).Is(HttpStatusCode.NotFound);
            (await As(Nico).Get("/api/getReview?eventRecordId=e1-r2")).Is(HttpStatusCode.NotFound);
            (await As(Nico).Get("/api/getReview?eventRecordId=unknown")).Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Publish_TheAuthorPublishesOnce_TheTextsAreTrimmed()
        {
            (await As(Dayt).Post("/api/publishReview", Review(7.5m, 2, (" Story ", " Line one\r\nLine two "), ("Music", "Great")))).Ok();
            var again = await As(Dayt).Post("/api/publishReview", Review());

            again.Is(HttpStatusCode.BadRequest);
            Assert.Equal("It can't be published now", again.Text);
            var review = (await As(Dayt).Get("/api/getReview?eventRecordId=e1-r0")).Ok().At("review");
            Assert.Equal(7.5, review.Num("score"));
            Assert.Equal(2, review.Int("visibility"));
            Assert.True(review.Date("publishedAt") > DateTimeOffset.UtcNow.AddMinutes(-1));
            Assert.Null(review.At("repliesPublishedAt"));
            Assert.Equal(new[] { ("Story", "Line one\nLine two", (string?)null), ("Music", "Great", null) },
                review.Arr("sections").Select(s => (s.Str("title"), s.Str("text"), s.Str("reply"))));
        }

        [Fact]
        public async Task Publish_TheScoreIsRequired_FromOneToTenInHalves()
        {
            Assert.Contains("Score the game", (await As(Dayt).Post("/api/publishReview", Review(null))).Is(HttpStatusCode.BadRequest).Body);
            (await As(Dayt).Post("/api/publishReview", Review(0.5m))).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/publishReview", Review(10.5m))).Is(HttpStatusCode.BadRequest);
            Assert.Contains("The score goes in steps of 0.5", (await As(Dayt).Post("/api/publishReview", Review(7.3m))).Is(HttpStatusCode.BadRequest).Body);
            Assert.Equal(0, await Factory.ScalarAsync("SELECT COUNT(*) FROM Reviews"));

            (await As(Dayt).Post("/api/publishReview", Review(1m))).Ok();

            Assert.Equal(1m, (await Db(db => db.Reviews.SingleAsync())).Score);
        }

        [Fact]
        public async Task Publish_OnlyByThePlayerWhoPassedTheGame()
        {
            (await As(Sliva).Post("/api/publishReview", Review())).Is(HttpStatusCode.Forbidden);
            (await As(Sliva).Post("/api/publishReview", new { eventRecordId = "e1-r1", score = 8, visibility = 2, sections = new[] { new { title = "T", text = "X" } } }))
                .Is(HttpStatusCode.BadRequest);
            (await As(Sliva).Post("/api/publishReview", new { eventRecordId = "unknown", score = 8, visibility = 2, sections = new[] { new { title = "T", text = "X" } } }))
                .Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Publish_IsValidated()
        {
            Assert.Contains("A review needs at least one section",
                (await As(Dayt).Post("/api/publishReview", new { eventRecordId = "e1-r0", visibility = 2, sections = Array.Empty<object>() })).Is(HttpStatusCode.BadRequest).Body);
            Assert.Contains("A review can have up to 20 sections",
                (await As(Dayt).Post("/api/publishReview", Review(8m, 2, Enumerable.Range(0, 21).Select(i => ($"T{i}", "X")).ToArray()))).Is(HttpStatusCode.BadRequest).Body);
            Assert.Contains("Every section needs a topic",
                (await As(Dayt).Post("/api/publishReview", Review(8m, 2, ("  ", "X")))).Is(HttpStatusCode.BadRequest).Body);
            Assert.Contains("Every section needs a text",
                (await As(Dayt).Post("/api/publishReview", Review(8m, 2, ("T", "")))).Is(HttpStatusCode.BadRequest).Body);
            (await As(Dayt).Post("/api/publishReview", Review(8m, 2, (new string('t', 101), "X")))).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/publishReview", Review(8m, 2, ("T", new string('x', 5001))))).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/publishReview", Review(8m, null))).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/publishReview", Review(8m, 7))).Is(HttpStatusCode.BadRequest);
            Assert.Equal(0, await Factory.ScalarAsync("SELECT COUNT(*) FROM Reviews"));
        }

        [Theory]
        [InlineData(ReviewVisibility.OnlyAuthor, false, false, false)]
        [InlineData(ReviewVisibility.AuthorAndGuesser, true, false, false)]
        [InlineData(ReviewVisibility.Guild, true, true, false)]
        [InlineData(ReviewVisibility.Everyone, true, true, true)]
        public async Task Visibility_DecidesWhoReadsTheReview(ReviewVisibility visibility, bool guesser, bool member, bool stranger)
        {
            await Seed(Make.Review("rev", "e1-r0", visibility: visibility));

            async Task<bool> Sees(string viewer) => (await As(viewer).Get("/api/getReview?eventRecordId=e1-r0")).Status == HttpStatusCode.OK;
            async Task<bool> Lists(string viewer) => (await As(viewer).Get("/api/getCommunityReviews?count=10")).Ok().Arr("reviews").Count == 1;

            Assert.Equal((true, guesser, member, stranger), (await Sees(Dayt), await Sees(Sliva), await Sees(Nico), await Sees(Stranger)));
            Assert.Equal((true, guesser, member, stranger), (await Lists(Dayt), await Lists(Sliva), await Lists(Nico), await Lists(Stranger)));
        }

        [Fact]
        public async Task Visibility_IsChangedByTheAuthorAtAnyTime()
        {
            await Seed(Make.Review("rev", "e1-r0", visibility: ReviewVisibility.Guild));

            (await As(Sliva).Post("/api/setReviewVisibility", new { eventRecordId = "e1-r0", visibility = 3 })).Is(HttpStatusCode.Forbidden);
            (await As(Dayt).Post("/api/setReviewVisibility", new { eventRecordId = "e1-r1", visibility = 3 })).Is(HttpStatusCode.NotFound);
            (await As(Dayt).Post("/api/setReviewVisibility", new { eventRecordId = "e1-r0", visibility = 9 })).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/setReviewVisibility", new { eventRecordId = "e1-r0", visibility = 3 })).Ok();

            Assert.Equal(ReviewVisibility.Everyone, (await Db(db => db.Reviews.SingleAsync())).Visibility);
            (await As(Stranger).Get("/api/getReview?eventRecordId=e1-r0")).Is(HttpStatusCode.OK);
        }

        [Fact]
        public async Task Replies_TheOneWhoGuessedAnswersOnce_SomeSectionsMayStayUnanswered()
        {
            await Seed(Make.Review("rev", "e1-r0", sections: new[] { ("Story", "Good", (string?)null), ("Music", "Great", null) }));

            (await As(Sliva).Post("/api/publishReviewReplies", new
            {
                eventRecordId = "e1-r0",
                replies = new[] { new { sectionId = "rev-s0", text = " Thanks\r\nfriend " }, new { sectionId = "rev-s1", text = "  " } },
            })).Ok();
            var again = await As(Sliva).Post("/api/publishReviewReplies", new { eventRecordId = "e1-r0", replies = new[] { new { sectionId = "rev-s1", text = "Late" } } });

            again.Is(HttpStatusCode.BadRequest);
            var review = (await As(Nico).Get("/api/getReview?eventRecordId=e1-r0")).Ok().At("review");
            Assert.Equal(new[] { "Thanks\nfriend", null }, review.Arr("sections").Select(s => s.Str("reply")));
            Assert.NotNull(review.Date("repliesPublishedAt"));
        }

        [Fact]
        public async Task Replies_AreRefused()
        {
            await Seed(Make.Review("rev", "e1-r0"));
            object Replies(string sectionId, string text = "Answer") => new { eventRecordId = "e1-r0", replies = new[] { new { sectionId, text } } };

            (await As(Dayt).Post("/api/publishReviewReplies", Replies("rev-s0"))).Is(HttpStatusCode.Forbidden);
            (await As(Sliva).Post("/api/publishReviewReplies", Replies("another-section"))).Is(HttpStatusCode.BadRequest);
            Assert.Contains("Answer at least one section", (await As(Sliva).Post("/api/publishReviewReplies", Replies("rev-s0", " "))).Is(HttpStatusCode.BadRequest).Body);
            (await As(Sliva).Post("/api/publishReviewReplies", Replies("rev-s0", new string('x', 2001)))).Is(HttpStatusCode.BadRequest);
            (await As(Sliva).Post("/api/publishReviewReplies", new { eventRecordId = "unknown", replies = new[] { new { sectionId = "rev-s0", text = "A" } } }))
                .Is(HttpStatusCode.NotFound);
            // the review isn't written yet
            (await As(Dayt).Post("/api/publishReviewReplies", new { eventRecordId = "e1-r1", replies = new[] { new { sectionId = "x", text = "A" } } }))
                .Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Replies_ToAReviewKeptByItsAuthor_AreForbidden()
        {
            await Seed(Make.Review("rev", "e1-r0", visibility: ReviewVisibility.OnlyAuthor));

            (await As(Sliva).Post("/api/publishReviewReplies", new { eventRecordId = "e1-r0", replies = new[] { new { sectionId = "rev-s0", text = "A" } } }))
                .Is(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task PlayerReviews_TheNewestFirst_FromAllGuilds()
        {
            await Seed(Make.Guild("g3"), Make.Member(Dayt, "g3"), Make.Member(Sliva, "g3"),
                Make.Event("e3", "g3", Sliva, EventStatusId.Finished), Make.Record("e3-r0", "e3", Sliva, Dayt, Sekiro, 2, DateTimeOffset.UtcNow.AddDays(-30)),
                Make.Review("old", "e3-r0", 3m, ReviewVisibility.Guild, DateTimeOffset.UtcNow.AddDays(-20)),
                Make.Review("new", "e1-r0", 9.5m, ReviewVisibility.AuthorAndGuesser, DateTimeOffset.UtcNow.AddDays(-1),
                    DateTimeOffset.UtcNow, ("Story", "Good", "Thanks")));

            var all = (await As(Sliva).Get($"/api/getPlayerReviews?userId={Dayt}")).Ok().Arr("reviews");
            var forNico = (await As(Nico).Get($"/api/getPlayerReviews?userId={Dayt}")).Ok().Arr("reviews");

            Assert.Equal(new[] { "e1-r0", "e3-r0" }, all.Select(r => r.Str("eventRecordId")));
            var first = all[0];
            Assert.Equal(("Spring", "Guild One", "Elden Ring", Sliva), (first.Str("eventName"), first.Str("guildName"), first.Str("game.name"), first.Str("guesser.id")));
            Assert.Equal(9.5, first.Num("score"));
            Assert.Equal(1, first.Int("visibility"));
            Assert.True(first.Bool("hasReplies"));
            Assert.Equal(3, all[1].Num("score"));
            Assert.False(all[1].Bool("hasReplies"));
            Assert.Empty(forNico);
        }

        [Fact]
        public async Task Suggestions_ReviewsToWrite_AndToAnswer()
        {
            await Seed(Make.Event("e2", G1, Nico, EventStatusId.Active),
                Make.Record("e2-r0", "e2", Nico, Dayt, Sekiro, 2, DateTimeOffset.UtcNow.AddDays(-1)),
                Make.Record("e2-r1", "e2", Dayt, Nico, Persona, 3, DateTimeOffset.UtcNow.AddDays(-3)),
                Make.Review("kept", "e2-r1", visibility: ReviewVisibility.OnlyAuthor));

            var dayt = (await As(Dayt).Get("/api/getReviewSuggestions")).Ok();
            await Seed(Make.Review("rev", "e1-r0"));
            var sliva = (await As(Sliva).Get("/api/getReviewSuggestions")).Ok();

            Assert.Equal(new[] { "e2-r0", "e1-r0" }, dayt.Strs("toWrite", "eventRecordId"));
            Assert.Equal(("Spring", "Elden Ring"), (dayt.Str("toWrite[1].eventName"), dayt.Str("toWrite[1].game.name")));
            // the review Nico keeps to himself isn't offered to Dayt to answer
            Assert.Empty(dayt.Arr("toAnswer"));
            Assert.Equal(new[] { "e1-r0" }, sliva.Strs("toAnswer", "eventRecordId"));
            Assert.Empty(sliva.Arr("toWrite"));
        }

        [Fact]
        public async Task Community_TheNewestFirst_ByPortions()
        {
            var rows = new List<object> { Make.Event("e5", G1, Sliva, EventStatusId.Finished) };
            for (var i = 0; i < 5; i++)
            {
                rows.Add(Make.Record($"e5-r{i}", "e5", Sliva, Dayt, Elden, 3));
                rows.Add(Make.Review($"rev{i}", $"e5-r{i}", i % 2 == 0 ? 8m : 3m, ReviewVisibility.Guild, DateTimeOffset.UtcNow.AddDays(-i)));
            }

            await Seed(rows.ToArray());

            var two = (await As(Nico).Get("/api/getCommunityReviews?count=2")).Ok();
            var all = (await As(Nico).Get("/api/getCommunityReviews?count=50")).Ok();
            var atLeastOne = (await As(Nico).Get("/api/getCommunityReviews?count=0")).Ok();

            Assert.Equal(new[] { "e5-r0", "e5-r1" }, two.Strs("reviews", "eventRecordId"));
            Assert.True(two.Bool("hasMore"));
            Assert.Equal(5, all.Arr("reviews").Count);
            Assert.False(all.Bool("hasMore"));
            Assert.Single(atLeastOne.Arr("reviews"));
            var first = two.At("reviews[0]");
            Assert.Equal(("Elden Ring", Dayt, (double?)8), (first.Str("game.name"), first.Str("author.id"), first.Num("score")));
        }

        [Fact]
        public async Task Reviews_OfADeletedEvent_AreHidden()
        {
            await Seed(Make.Review("rev", "e1-r0", visibility: ReviewVisibility.Everyone));
            (await As(Admin).Post("/api/deleteEvent", new { eventId = "e1" })).Ok();

            (await As(Dayt).Get("/api/getReview?eventRecordId=e1-r0")).Is(HttpStatusCode.NotFound);
            Assert.Empty((await As(Dayt).Get("/api/getCommunityReviews?count=10")).Ok().Arr("reviews"));
            Assert.Empty((await As(Sliva).Get($"/api/getPlayerReviews?userId={Dayt}")).Ok().Arr("reviews"));
        }
    }
}
