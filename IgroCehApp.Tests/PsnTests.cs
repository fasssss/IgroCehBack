using System.Net;
using System.Web;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // PSN through the service account of the site: a player links their account by a code in About me,
    // the administrator reads the trophies of games and the state of the service account
    public class PsnTests : ApiTest
    {
        private const string AccountId = "1234567890123456789";
        private const string TrophiesUrl = ApiFactory.PsnApiAddress + "/api/trophy/v1/npCommunicationIds/NPWR20188_00/trophyGroups/all/trophies";

        public PsnTests(ApiFactory factory) : base(factory)
        {
        }

        public override async Task InitializeAsync()
        {
            // the factory signs the service account out before every test
            await base.InitializeAsync();
            await Seed(Make.User("dayt", "Dayt"), Make.User("boss", "Boss", isAppAdmin: true));
        }

        private void ServiceAccountSignsIn(string location = ApiFactory.PsnRedirectUri + "/?code=v3.CODE&cid=1")
        {
            External.On(HttpMethod.Get, $"{ApiFactory.PsnAuthAddress}/api/authz/v3/oauth/authorize", (_, _) =>
            {
                var response = new HttpResponseMessage(HttpStatusCode.Found);
                response.Headers.Location = new Uri(location);
                return response;
            });
            External.OnPost($"{ApiFactory.PsnAuthAddress}/api/authz/v3/oauth/token",
                new { access_token = "psn-access", expires_in = 3600, refresh_token = "psn-refresh", refresh_token_expires_in = 5184000 });
        }

        private void ProfileIs(string onlineId, string aboutMe) =>
            External.OnGet($"{ApiFactory.PsnProfileAddress}/userProfile/v1/users/{onlineId}/profile2", new { profile = new { accountId = AccountId, onlineId, aboutMe } });

        [Fact]
        public async Task Start_GivesACode_ThatStaysTheSameUntilTheLink()
        {
            var code = (await As("dayt").Post("/api/startPsnLink", new { onlineId = "DaytPsn" })).Ok().Str("code")!;
            var again = (await As("dayt").Post("/api/startPsnLink", new { onlineId = "Other_Id" })).Ok().Str("code");

            Assert.Matches("^IGRO-[23456789ABCDEFGHJKMNPQRSTUVWXYZ]{6}$", code);
            Assert.Equal(code, again);
            (await As("dayt").Post("/api/startPsnLink", new { onlineId = "a" })).Is(HttpStatusCode.BadRequest);
            (await As("dayt").Post("/api/startPsnLink", new { onlineId = "has space" })).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Confirm_TheCodeInAboutMe_LinksTheAccount()
        {
            ServiceAccountSignsIn();
            var code = (await As("dayt").Post("/api/startPsnLink", new { onlineId = "DaytPsn" })).Ok().Str("code")!;
            ProfileIs("DaytPsn", $"Hello! {code.ToLowerInvariant()}");

            Assert.Equal("DaytPsn", (await As("dayt").Post("/api/confirmPsnLink")).Ok().Str("psnOnlineId"));

            var user = await Db(db => db.Users.SingleAsync(u => u.Id == "dayt"));
            Assert.Equal((AccountId, "DaytPsn"), (user.PsnAccountId, user.PsnOnlineId));
            var authorize = External.Requests.Single(r => r.Address.EndsWith("/oauth/authorize"));
            Assert.Equal("npsso=test-npsso", authorize.Headers["Cookie"]);
            Assert.Contains("client_id=test-psn-client", authorize.Query);
            var token = External.Requests.Single(r => r.Address.EndsWith("/oauth/token"));
            Assert.Contains("code=v3.CODE", token.Body);
            Assert.Contains("grant_type=authorization_code", token.Body);
            Assert.Equal("Bearer psn-access", External.Requests.Single(r => r.Address.Contains("/profile2")).Headers["Authorization"]);
            // the pending link is gone
            (await As("dayt").Post("/api/confirmPsnLink")).Is(HttpStatusCode.Gone);
        }

        [Fact]
        public async Task Confirm_WithoutAStart_IsGone()
        {
            (await As("dayt").Post("/api/confirmPsnLink")).Is(HttpStatusCode.Gone);
        }

        [Fact]
        public async Task Confirm_WithoutTheCode_IsRefused_AndCanBeRetriedOnlyLater()
        {
            ServiceAccountSignsIn();
            (await As("dayt").Post("/api/startPsnLink", new { onlineId = "DaytPsn" })).Ok();
            ProfileIs("DaytPsn", "no code here");

            Assert.Equal("About me doesn't show the code", (await As("dayt").Post("/api/confirmPsnLink")).Is(HttpStatusCode.BadRequest).Text);
            (await As("dayt").Post("/api/confirmPsnLink")).Is(HttpStatusCode.TooManyRequests);
        }

        [Fact]
        public async Task Confirm_AnUnknownAccount_IsNotFound()
        {
            ServiceAccountSignsIn();
            (await As("dayt").Post("/api/startPsnLink", new { onlineId = "Nobody" })).Ok();
            External.OnGet($"{ApiFactory.PsnProfileAddress}/userProfile/v1/users/Nobody/profile2", new { error = "not found" }, HttpStatusCode.NotFound);

            Assert.Equal("PSN has no account with this online id", (await As("dayt").Post("/api/confirmPsnLink")).Is(HttpStatusCode.NotFound).Text);
        }

        [Fact]
        public async Task Confirm_WhenTheServiceAccountIsRefused_Is503_AndTheStatusSaysSo()
        {
            ServiceAccountSignsIn(ApiFactory.PsnRedirectUri + "/?error=login_required&error_code=4165");
            External.OnGet($"{ApiFactory.PsnAuthAddress}/api/v1/ssocookie", new { error = "invalid_grant" }, HttpStatusCode.BadRequest);
            (await As("dayt").Post("/api/startPsnLink", new { onlineId = "DaytPsn" })).Ok();

            (await As("dayt").Post("/api/confirmPsnLink")).Is(HttpStatusCode.ServiceUnavailable);
            var status = (await As("boss").Get("/api/getPsnStatus")).Ok().At("psnStatus");

            Assert.False(status.Bool("isSignInWorking"));
            Assert.True(status.Bool("isNpssoRefused"));
        }

        [Fact]
        public async Task Status_OfAWorkingServiceAccount()
        {
            ServiceAccountSignsIn();
            External.OnGet($"{ApiFactory.PsnAuthAddress}/api/v1/ssocookie", new { expires_in = 30L * 24 * 3600 });

            var status = (await As("boss").Get("/api/getPsnStatus")).Ok().At("psnStatus");

            Assert.True(status.Bool("isSignInWorking"));
            Assert.False(status.Bool("isNpssoRefused"));
            Assert.False(status.Bool("isNpssoEnding"));
            Assert.InRange(status.Date("npssoExpiresAt")!.Value, DateTimeOffset.UtcNow.AddDays(29), DateTimeOffset.UtcNow.AddDays(31));
            Assert.NotNull(status.Date("refreshTokenExpiresAt"));
        }

        [Fact]
        public async Task Status_AnNpssoEndingInAFewDays_IsReported()
        {
            ServiceAccountSignsIn();
            External.OnGet($"{ApiFactory.PsnAuthAddress}/api/v1/ssocookie", new { expires_in = 3L * 24 * 3600 });

            Assert.True((await As("boss").Get("/api/getPsnStatus")).Ok().Bool("psnStatus.isNpssoEnding"));
        }

        [Fact]
        public async Task Trophies_OfAnOlderGame_AreFoundInTheOlderService()
        {
            ServiceAccountSignsIn();
            External.On(HttpMethod.Get, TrophiesUrl, (request, _) =>
            {
                if (HttpUtility.ParseQueryString(request.RequestUri!.Query)["npServiceName"] == "trophy2")
                {
                    return FakeExternalApis.Json(new { error = "not found" }, HttpStatusCode.NotFound);
                }

                var isRussian = request.Headers.AcceptLanguage.ToString() == "ru-RU";
                return FakeExternalApis.Json(new
                {
                    trophies = new[]
                    {
                        new { trophyId = 1, trophyHidden = true, trophyName = isRussian ? "Первый" : "First", trophyDetail = isRussian ? "" : "First detail", trophyIconUrl = "https://psn.test/1.png" },
                        new { trophyId = 0, trophyHidden = false, trophyName = isRussian ? "Платина" : "Platinum", trophyDetail = isRussian ? "Всё" : "All", trophyIconUrl = "https://psn.test/0.png" },
                    },
                    nextOffset = (int?)null,
                });
            });
            External.OnGet($"{ApiFactory.PsnApiAddress}/api/trophy/v1/npCommunicationIds/NPWR20188_00/trophyGroups", new { trophyTitleName = "Old Game" });

            var answer = (await As("boss").Get("/api/getPsnTrophies?titleRef=npwr20188_00")).Ok();
            (await As("boss").Get("/api/getPsnTrophies?titleRef=NPWR20188_00")).Ok();

            Assert.Equal(("NPWR20188_00/trophy", "Old Game"), (answer.Str("titleRef"), answer.Str("titleName")));
            var achievements = answer.Arr("achievements");
            Assert.Equal(new[] { ("0", "Platinum", "Платина", "Всё", false), ("1", "First", "Первый", "First detail", true) },
                achievements.Select(a => (a.Str("apiName"), a.Str("name"), a.Str("nameRu"), a.Str("descriptionRu"), a.Bool("isHidden"))));
            // the trophies are cached, the newer service is asked once
            Assert.Equal(1, External.Requests.Count(r => r.Address == TrophiesUrl && r.Query.Contains("trophy2")));
        }

        [Fact]
        public async Task Trophies_AreChecked()
        {
            ServiceAccountSignsIn();
            External.OnGet(TrophiesUrl, new { error = "not found" }, HttpStatusCode.NotFound);

            (await As("boss").Get("/api/getPsnTrophies?titleRef=NPWR20188_00")).Is(HttpStatusCode.NotFound);
            Assert.Contains("The id of a trophy list looks like NPWR20188_00", (await As("boss").Get("/api/getPsnTrophies?titleRef=CUSA00001")).Is(HttpStatusCode.BadRequest).Body);
            (await As("dayt").Get("/api/getPsnTrophies?titleRef=NPWR20188_00")).Is(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Unlink()
        {
            await Db(async db =>
            {
                var user = db.Users.Find("dayt")!;
                user.PsnAccountId = AccountId;
                user.PsnOnlineId = "DaytPsn";
                await db.SaveChangesAsync();
            });

            (await As("dayt").Post("/api/unlinkPsn")).Ok();

            var saved = await Db(db => db.Users.SingleAsync(u => u.Id == "dayt"));
            Assert.Equal((null, null), (saved.PsnAccountId, saved.PsnOnlineId));
        }
    }
}
