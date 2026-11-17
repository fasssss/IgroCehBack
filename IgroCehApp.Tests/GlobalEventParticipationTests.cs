using System.Net;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // a player in a global event: rolls three games, picks one, proves its passing by the achievements of a platform or by a claim
    public class GlobalEventParticipationTests : GlobalEventTest
    {
        private const string DaytSteam = "76561198000000002";

        public GlobalEventParticipationTests(ApiFactory factory) : base(factory)
        {
        }

        private Task<ApiResult> Check(string globalEventId, int platform = 1, string player = Dayt) =>
            As(player).Post("/api/checkGlobalEventPassing", new { globalEventId, platform });

        private Task SetPickedAt(DateTimeOffset pickedAt) => Db(async db =>
        {
            db.GlobalEventParticipants.Single().PickedAt = pickedAt;
            await db.SaveChangesAsync();
        });

        [Fact]
        public async Task Roll_ThreeGames_Once()
        {
            var id = await CreateGlobalEvent();
            Assert.Null((await As(Dayt).Get($"/api/getGlobalEventParticipation?globalEventId={id}")).Ok().At("participation"));

            var participation = (await As(Dayt).Post("/api/rollGlobalEventGames", new { globalEventId = id })).Ok().At("participation");

            Assert.Equal(new[] { 10, 20, 30 }, participation.Ints("rolledAppIds").OrderBy(appId => appId));
            Assert.Null(participation.At("pickedAppId"));
            Assert.NotNull(participation.Date("rolledAt"));
            IsConflict(await As(Dayt).Post("/api/rollGlobalEventGames", new { globalEventId = id }), "The games are rolled already");
            Assert.Equal(participation.Ints("rolledAppIds"),
                (await As(Dayt).Get($"/api/getGlobalEventParticipation?globalEventId={id}")).Ok().Ints("participation.rolledAppIds"));
            Assert.Equal(1, (await As(Nico).Get($"/api/getGlobalEvent?globalEventId={id}")).Ok().Int("globalEvent.participantCount"));
        }

        [Fact]
        public async Task Roll_IsRefused_InFinishedAndUnknownEvents()
        {
            var id = await CreateGlobalEvent();
            (await As(Boss).Post("/api/finishGlobalEvent", new { globalEventId = id })).Ok();

            IsConflict(await As(Dayt).Post("/api/rollGlobalEventGames", new { globalEventId = id }), "The event is finished");
            (await As(Dayt).Post("/api/rollGlobalEventGames", new { globalEventId = "unknown" })).Is(HttpStatusCode.NotFound);
            (await As(Dayt).Get("/api/getGlobalEventParticipation?globalEventId=unknown")).Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Roll_LeavesOutTheGamesThePlayerHasPassedOnSteam()
        {
            var id = await CreateGlobalEvent();
            await LinkSteam(Dayt, DaytSteam);
            Steam.Owns(DaytSteam, 10, 20).Unlocked(DaytSteam, 10, ("A2", DateTimeOffset.UtcNow.AddYears(-1))).Unlocked(DaytSteam, 20);

            var rolled = (await As(Dayt).Post("/api/rollGlobalEventGames", new { globalEventId = id })).Ok().Ints("participation.rolledAppIds");

            Assert.Equal(new[] { 20, 30 }, rolled.OrderBy(appId => appId));
        }

        [Fact]
        public async Task Roll_WhenEveryGameIsPassed_Is422()
        {
            var id = await CreateGlobalEvent(NewEvent(games: new[] { SteamGame(10, "A1"), SteamGame(20, "B1"), SteamGame(40) }));
            await Db(async db =>
            {
                // the game 40 has no achievements, it is passed only by a claim, so here it is dropped from the event by hand
                var globalEvent = await db.GlobalEvents.Include(ge => ge.Games).ThenInclude(g => g.Game).SingleAsync();
                db.RemoveRange(globalEvent.Games.Where(g => g.Game.VgcId == 40));
                await db.SaveChangesAsync();
            });
            await LinkSteam(Dayt, DaytSteam);
            Steam.Owns(DaytSteam, 10, 20).Unlocked(DaytSteam, 10, ("A1", DateTimeOffset.UtcNow)).Unlocked(DaytSteam, 20, ("B1", DateTimeOffset.UtcNow));

            var answer = await As(Dayt).Post("/api/rollGlobalEventGames", new { globalEventId = id });

            Assert.Equal("The player has unlocked an achievement of every game they could get", answer.Is(HttpStatusCode.UnprocessableEntity).Text);
        }

        [Fact]
        public async Task Roll_AHiddenSteamProfile_DoesntHideAnyGame()
        {
            var id = await CreateGlobalEvent();
            await LinkSteam(Dayt, DaytSteam);
            Steam.HidesGames(DaytSteam);

            Assert.Equal(3, (await As(Dayt).Post("/api/rollGlobalEventGames", new { globalEventId = id })).Ok().Arr("participation.rolledAppIds").Count);
        }

        [Fact]
        public async Task Pick_OneOfTheRolledGames_Once()
        {
            var id = await CreateGlobalEvent();

            IsConflict(await As(Dayt).Post("/api/pickGlobalEventGame", new { globalEventId = id, appId = 10 }), "The games aren't rolled yet");
            (await As(Dayt).Post("/api/rollGlobalEventGames", new { globalEventId = id })).Ok();
            Assert.Equal("The game isn't among the rolled ones",
                (await As(Dayt).Post("/api/pickGlobalEventGame", new { globalEventId = id, appId = 40 })).Is(HttpStatusCode.BadRequest).Text);
            var picked = (await As(Dayt).Post("/api/pickGlobalEventGame", new { globalEventId = id, appId = 20 })).Ok().At("participation");

            Assert.Equal(20, picked.Int("pickedAppId"));
            Assert.Equal(picked.Date("pickedAt")!.Value.AddDays(7), picked.Date("refusableAt"));
            IsConflict(await As(Dayt).Post("/api/pickGlobalEventGame", new { globalEventId = id, appId = 10 }), "A game is picked already");
        }

        [Fact]
        public async Task Refuse_AWeekAfterThePick_RollsAgainWithoutThatGame()
        {
            var id = await CreateGlobalEvent();
            await Pick(id, Dayt, 10);

            IsConflict(await As(Dayt).Post("/api/refuseGlobalEventGame", new { globalEventId = id }), "The picked game can't be refused yet");
            await SetPickedAt(DateTimeOffset.UtcNow.AddDays(-7).AddMinutes(-1));
            var refused = (await As(Dayt).Post("/api/refuseGlobalEventGame", new { globalEventId = id })).Ok().At("participation");

            Assert.Equal(new[] { 20, 30 }, refused.Ints("rolledAppIds").OrderBy(appId => appId));
            Assert.Null(refused.At("pickedAppId"));
            Assert.Null(refused.At("refusableAt"));
            IsConflict(await As(Dayt).Post("/api/refuseGlobalEventGame", new { globalEventId = id }), "No game is picked");
            IsConflict(await As(Nico).Post("/api/refuseGlobalEventGame", new { globalEventId = id }), "No game is picked");
        }

        [Fact]
        public async Task Check_AnAchievementUnlockedAfterThePick_PassesTheGame()
        {
            var id = await CreateGlobalEvent();
            await Pick(id, Dayt, 10);
            await LinkSteam(Dayt, DaytSteam);
            Steam.Unlocked(DaytSteam, 10, ("A1", DateTimeOffset.UtcNow.AddYears(-1)), ("A2", DateTimeOffset.UtcNow.AddSeconds(1)));

            var answer = (await Check(id)).Ok();

            Assert.Equal(0, answer.Int("passingCheck"));
            var participation = answer.At("participation");
            Assert.NotNull(participation.Date("passedAt"));
            Assert.Equal(1, participation.Int("passedPlatform"));
            var check = Assert.Single(participation.Arr("passingChecks"));
            Assert.Equal(check.Date("checkedAt")!.Value.AddMinutes(10), check.Date("checkableAt"));
            Assert.Equal(1, (await As(Nico).Get($"/api/getGlobalEvent?globalEventId={id}")).Ok().Int("globalEvent.passedCount"));
            IsConflict(await Check(id), "The picked game is passed already");
        }

        [Theory]
        [InlineData(-1, 2)]
        [InlineData(null, 1)]
        public async Task Check_WithoutANewAchievement_DoesntPass(int? unlockedDaysAgo, int result)
        {
            var id = await CreateGlobalEvent();
            await Pick(id, Dayt, 10);
            await LinkSteam(Dayt, DaytSteam);
            // A1 doesn't count, only the chosen A2 does
            Steam.Unlocked(DaytSteam, 10, unlockedDaysAgo == null
                ? new[] { ("A1", DateTimeOffset.UtcNow.AddMinutes(1)) }
                : new[] { ("A2", DateTimeOffset.UtcNow.AddDays(unlockedDaysAgo.Value)) });

            var answer = (await Check(id)).Ok();

            Assert.Equal(result, answer.Int("passingCheck"));
            Assert.Null(answer.At("participation.passedAt"));
        }

        [Fact]
        public async Task Check_AHiddenProfile_IsReported()
        {
            var id = await CreateGlobalEvent();
            await Pick(id, Dayt, 10);
            await LinkSteam(Dayt, DaytSteam);
            Steam.HidesAchievements(DaytSteam, 10);

            Assert.Equal(3, (await Check(id)).Ok().Int("passingCheck"));
        }

        [Fact]
        public async Task Check_IsRepeatedNotOftenerThanEveryTenMinutes()
        {
            var id = await CreateGlobalEvent();
            await Pick(id, Dayt, 10);
            await LinkSteam(Dayt, DaytSteam);
            Steam.Unlocked(DaytSteam, 10);

            (await Check(id)).Ok();
            (await Check(id)).Is(HttpStatusCode.TooManyRequests);
            await Db(async db =>
            {
                db.GlobalEventPassingChecks.Single().CheckedAt = DateTimeOffset.UtcNow.AddMinutes(-11);
                await db.SaveChangesAsync();
            });
            (await Check(id)).Ok();
        }

        [Fact]
        public async Task Check_WhenSteamFails_Is503_AndCanBeRepeatedAtOnce()
        {
            var id = await CreateGlobalEvent();
            await Pick(id, Dayt, 10);
            await LinkSteam(Dayt, DaytSteam);
            External.OnGet($"{ApiFactory.SteamAddress}/ISteamUserStats/GetPlayerAchievements/v1/", new { }, HttpStatusCode.InternalServerError);

            (await Check(id)).Is(HttpStatusCode.ServiceUnavailable);

            Assert.Equal(0, await Factory.ScalarAsync("SELECT COUNT(*) FROM GlobalEventPassingChecks"));
        }

        [Fact]
        public async Task Check_IsRefused()
        {
            var id = await CreateGlobalEvent();

            IsConflict(await Check(id), "No game is picked");
            (await Check("unknown")).Is(HttpStatusCode.NotFound);
            await Pick(id, Dayt, 10);
            IsConflict(await Check(id), "Steam isn't linked");
            IsConflict(await Check(id, platform: 2), "The game has no achievements in Xbox");
            await Pick(id, Nico, 30);
            IsConflict(await Check(id, player: Nico), "The game has no achievements, only a claim proves its passing");
            (await Check(id, platform: 9)).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Claim_TheAdministratorApprovesIt_AndThePlayerGetsTheBadge()
        {
            var id = await CreateGlobalEvent();
            await Pick(id, Dayt, 30);

            var claim = (await As(Dayt).Post("/api/submitGlobalEventClaim", new { globalEventId = id, platform = 3, profile = "  steamcommunity.com/id/dayt " })).Ok()
                .At("participation.claim");
            IsConflict(await As(Dayt).Post("/api/submitGlobalEventClaim", new { globalEventId = id, platform = 3, profile = "again" }), "A claim is under review");
            var claims = (await As(Boss).Get($"/api/getGlobalEventClaims?globalEventId={id}")).Ok().Arr("claims");
            (await As(Boss).Post("/api/decideGlobalEventClaim", new { claimId = claim.Str("id"), isApproved = true, comment = "ignored" })).Ok();
            var decidedAgain = await As(Boss).Post("/api/decideGlobalEventClaim", new { claimId = claim.Str("id"), isApproved = false });

            Assert.Equal((30, 3, "steamcommunity.com/id/dayt", 0), (claim.Int("appId"), claim.Int("platform"), claim.Str("profile"), claim.Int("status")));
            Assert.Equal((Dayt, "Dayt"), (claims[0].Str("player.id"), claims[0].Str("player.userName")));
            IsConflict(decidedAgain, "The claim is decided already");
            var participation = (await As(Dayt).Get($"/api/getGlobalEventParticipation?globalEventId={id}")).Ok().At("participation");
            Assert.NotNull(participation.Date("passedAt"));
            Assert.Equal((1, null), (participation.Int("claim.status"), participation.Str("claim.comment")));
            var decided = (await As(Boss).Get($"/api/getGlobalEventClaims?globalEventId={id}")).Ok().At("claims[0]");
            Assert.Equal((Boss, 1), (decided.Str("decidedBy.id"), decided.Int("status")));
            // the profile opens only through a guild, even the own one (current behavior)
            (await As(Dayt).Get($"/api/getPlayerProfile?userId={Dayt}")).Is(HttpStatusCode.NotFound);
            await Seed(Make.Guild("g1"), Make.Member(Dayt, "g1"));
            var badge = Assert.Single((await As(Dayt).Get($"/api/getPlayerProfile?userId={Dayt}")).Ok().Arr("playerProfile.badges"));
            Assert.Equal((KnightBadge, id, "Ladder"), (badge.Str("badgeId"), badge.Str("globalEventId"), badge.Str("globalEventName")));
        }

        [Fact]
        public async Task Claim_Rejected_WithAComment_ThePlayerMaySubmitAnother()
        {
            var id = await CreateGlobalEvent();
            await Pick(id, Dayt, 30);
            var claimId = (await As(Dayt).Post("/api/submitGlobalEventClaim", new { globalEventId = id, platform = 1, profile = "dayt-psn" })).Ok().Str("participation.claim.id");

            (await As(Boss).Post("/api/decideGlobalEventClaim", new { claimId, isApproved = false, comment = " No trophy\r\nyet " })).Ok();
            var rejected = (await As(Dayt).Get($"/api/getGlobalEventParticipation?globalEventId={id}")).Ok().At("participation");
            var second = (await As(Dayt).Post("/api/submitGlobalEventClaim", new { globalEventId = id, platform = 2, profile = "dayt-xbox" })).Ok().At("participation");

            Assert.Equal((2, "No trophy\nyet"), (rejected.Int("claim.status"), rejected.Str("claim.comment")));
            Assert.Null(rejected.At("passedAt"));
            Assert.Equal((0, "dayt-xbox"), (second.Int("claim.status"), second.Str("claim.profile")));
            var claims = (await As(Boss).Get($"/api/getGlobalEventClaims?globalEventId={id}")).Ok().Arr("claims");
            // the pending claims come first
            Assert.Equal(new[] { 0, 2 }, claims.Select(c => c.Int("status")));
        }

        [Fact]
        public async Task Claim_IsChecked()
        {
            var id = await CreateGlobalEvent();

            IsConflict(await As(Dayt).Post("/api/submitGlobalEventClaim", new { globalEventId = id, platform = 1, profile = "x" }), "No game is picked");
            await Pick(id, Dayt, 30);
            (await As(Dayt).Post("/api/submitGlobalEventClaim", new { globalEventId = id, platform = 1, profile = "  " })).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/submitGlobalEventClaim", new { globalEventId = id, platform = 7, profile = "x" })).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/submitGlobalEventClaim", new { globalEventId = id, profile = "x" })).Is(HttpStatusCode.BadRequest);
            (await As(Dayt).Post("/api/submitGlobalEventClaim", new { globalEventId = id, platform = 1, profile = new string('x', 201) })).Is(HttpStatusCode.BadRequest);
            (await As(Boss).Post("/api/decideGlobalEventClaim", new { claimId = "unknown", isApproved = true })).Is(HttpStatusCode.NotFound);
            (await As(Boss).Post("/api/decideGlobalEventClaim", new { claimId = "unknown" })).Is(HttpStatusCode.BadRequest);
            (await As(Boss).Get("/api/getGlobalEventClaims?globalEventId=unknown")).Is(HttpStatusCode.NotFound);
        }
    }
}
