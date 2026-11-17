using System.Net;
using System.Web;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // a player links their Steam account by signing in to Steam (OpenID); the site then reads the games and achievements of the account
    public class SteamLinkTests : ApiTest
    {
        private const string SteamId = "76561198000000002";
        private const string LoginAddress = ApiFactory.SteamCommunityAddress + "/openid/login";

        public SteamLinkTests(ApiFactory factory) : base(factory)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await Seed(Make.User("dayt", "Dayt"), Make.User("nico", "Nico"));
        }

        // the link to Steam and the query Steam sends the player back with, signed as Steam signs it
        private async Task<Dictionary<string, string>> SignInToSteam(string userId = "dayt", string returnPath = "/user/dayt")
        {
            var link = (await As(userId).Get($"/api/getSteamSignInLink?returnPath={Uri.EscapeDataString(returnPath)}")).Ok().Str("steamSignInLink")!;
            var returnTo = HttpUtility.ParseQueryString(new Uri(link).Query)["openid.return_to"]!;
            var back = HttpUtility.ParseQueryString(new Uri(returnTo).Query);
            return new Dictionary<string, string>
            {
                ["returnPath"] = back["returnPath"]!,
                ["state"] = back["state"]!,
                ["openid.ns"] = "http://specs.openid.net/auth/2.0",
                ["openid.mode"] = "id_res",
                ["openid.op_endpoint"] = LoginAddress,
                ["openid.claimed_id"] = $"https://steamcommunity.com/openid/id/{SteamId}",
                ["openid.identity"] = $"https://steamcommunity.com/openid/id/{SteamId}",
                ["openid.return_to"] = returnTo,
                ["openid.response_nonce"] = "2026-11-16T12:00:00Zabc",
                ["openid.assoc_handle"] = "1234567890",
                ["openid.signed"] = "signed,op_endpoint,claimed_id,identity,return_to,response_nonce,assoc_handle",
                ["openid.sig"] = "signature",
            };
        }

        private void SteamConfirms(bool isValid = true) =>
            External.On(HttpMethod.Post, LoginAddress, (_, _) => FakeExternalApis.Text($"ns:http://specs.openid.net/auth/2.0\nis_valid:{(isValid ? "true" : "false")}\n"));

        [Fact]
        public async Task SignInLink_LeadsToSteam_AndBackToTheLinkPage()
        {
            var link = new Uri((await As("dayt").Get("/api/getSteamSignInLink?returnPath=/user/dayt")).Ok().Str("steamSignInLink")!);
            var query = HttpUtility.ParseQueryString(link.Query);

            Assert.Equal(LoginAddress, link.GetLeftPart(UriPartial.Path));
            Assert.Equal(("checkid_setup", ApiFactory.FrontendUrl), (query["openid.mode"], query["openid.realm"]));
            Assert.StartsWith($"{ApiFactory.FrontendUrl}/steam-link?returnPath=%2Fuser%2Fdayt&state=", query["openid.return_to"]);
            Assert.Equal("http://specs.openid.net/auth/2.0/identifier_select", query["openid.claimed_id"]);
        }

        [Theory]
        [InlineData("https://evil.test/")]
        [InlineData("//evil.test")]
        [InlineData("relative")]
        public async Task SignInLink_ReturnsOnlyToThePagesOfTheSite(string returnPath)
        {
            (await As("dayt").Get($"/api/getSteamSignInLink?returnPath={Uri.EscapeDataString(returnPath)}")).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Link_SteamConfirmsTheSignIn_TheAccountIsSaved()
        {
            var parameters = await SignInToSteam();
            SteamConfirms();
            new FakeSteam(External).Owns(SteamId, 10);

            var answer = (await As("dayt").Post("/api/linkSteam", new { parameters })).Ok();

            Assert.True(answer.Bool("isGameDataPublic"));
            Assert.Equal(SteamId, (await Db(db => db.Users.SingleAsync(u => u.Id == "dayt"))).SteamId);
            var check = External.Requests.Single(r => r.Address == LoginAddress);
            Assert.Contains("openid.mode=check_authentication", check.Body);
            Assert.Equal(SteamId, (await As("dayt").Get("/api/getUserObject")).Ok().Str("steamId"));
        }

        [Fact]
        public async Task Link_WithHiddenGameDetails_TellsSo()
        {
            var parameters = await SignInToSteam();
            SteamConfirms();
            new FakeSteam(External).HidesGames(SteamId);

            Assert.False((await As("dayt").Post("/api/linkSteam", new { parameters })).Ok().Bool("isGameDataPublic"));
            Assert.False((await As("dayt").Get("/api/getSteamProfileStatus")).Ok().Bool("isGameDataPublic"));
        }

        [Fact]
        public async Task Link_WhenTheGamesCantBeRead_TheAccountIsLinkedAnyway()
        {
            var parameters = await SignInToSteam();
            SteamConfirms();
            External.OnGet($"{ApiFactory.SteamAddress}/IPlayerService/GetOwnedGames/v1/", new { }, HttpStatusCode.InternalServerError);

            var answer = (await As("dayt").Post("/api/linkSteam", new { parameters })).Ok();

            Assert.Null(answer.At("isGameDataPublic"));
            Assert.Equal(SteamId, (await Db(db => db.Users.SingleAsync(u => u.Id == "dayt"))).SteamId);
            (await As("dayt").Get("/api/getSteamProfileStatus")).Is(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task Link_SteamDoesntConfirm_IsRefused()
        {
            var parameters = await SignInToSteam();
            SteamConfirms(isValid: false);

            Assert.Equal("Steam doesn't confirm the sign-in", (await As("dayt").Post("/api/linkSteam", new { parameters })).Is(HttpStatusCode.BadRequest).Text);
            Assert.Null((await Db(db => db.Users.SingleAsync(u => u.Id == "dayt"))).SteamId);
        }

        [Fact]
        public async Task Link_AForgedAnswer_IsRefusedWithoutAskingSteam()
        {
            var parameters = await SignInToSteam();
            parameters["openid.op_endpoint"] = "https://evil.test/openid/login";

            (await As("dayt").Post("/api/linkSteam", new { parameters })).Is(HttpStatusCode.BadRequest);
            Assert.Equal(0, External.CountOf(LoginAddress));
        }

        [Fact]
        public async Task Link_TheStateOfAnotherPlayer_IsRefused()
        {
            var parameters = await SignInToSteam("nico");

            Assert.Equal("The sign-in is outdated or was started by another user",
                (await As("dayt").Post("/api/linkSteam", new { parameters })).Is(HttpStatusCode.BadRequest).Text);
            parameters["state"] = "broken";
            (await As("nico").Post("/api/linkSteam", new { parameters })).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Link_WhenSteamCantBeAsked_Is503()
        {
            var parameters = await SignInToSteam();
            External.On(HttpMethod.Post, LoginAddress, (_, _) => FakeExternalApis.Text("down", HttpStatusCode.BadGateway));

            (await As("dayt").Post("/api/linkSteam", new { parameters })).Is(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task Status_WithoutALinkedAccount_IsNotFound()
        {
            (await As("dayt").Get("/api/getSteamProfileStatus")).Is(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task Unlink()
        {
            await Db(async db =>
            {
                db.Users.Find("dayt")!.SteamId = SteamId;
                await db.SaveChangesAsync();
            });

            (await As("dayt").Post("/api/unlinkSteam")).Ok();

            Assert.Null((await Db(db => db.Users.SingleAsync(u => u.Id == "dayt"))).SteamId);
        }
    }
}
