using System.Net;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // the sign-in through Discord: the code of the redirect gives the user, their guilds and the cookie with the token of the site
    public class AuthorizationTests : ApiTest
    {
        public AuthorizationTests(ApiFactory factory) : base(factory)
        {
        }

        private void DiscordSignsIn(string id, string name, string? avatar, params (string Id, string Name, string? Icon, string Permissions)[] guilds)
        {
            External.OnPost($"{ApiFactory.DiscordAddress}/api/v10/oauth2/token", new
            {
                access_token = "discord-access",
                token_type = "Bearer",
                expires_in = 604800,
                refresh_token = "discord-refresh",
                scope = "identify email guilds",
            });
            External.OnGet($"{ApiFactory.DiscordAddress}/api/v10/users/@me", new { id, username = name, global_name = name, avatar, email = $"{id}@discord.test" });
            External.OnGet($"{ApiFactory.DiscordAddress}/api/v10/users/@me/guilds",
                guilds.Select(g => new { id = g.Id, name = g.Name, icon = g.Icon, permissions = g.Permissions }).ToArray());
        }

        [Fact]
        public async Task DiscordAuthLink_IsGivenToAnyone()
        {
            var link = (await Anonymous.Get("/api/getDiscordAuthLink")).Ok().Str("discordApiLink");

            Assert.Equal($"{ApiFactory.DiscordAddress}/oauth2/authorize?response_type=code&client_id=test-discord-client&scope=identify%20email%20guilds"
                + $"&redirect_uri={ApiFactory.FrontendUrl}/auth&prompt=consent", link);
        }

        [Fact]
        public async Task SignIn_CreatesTheUserAndTheirGuilds_AndSetsTheCookie()
        {
            DiscordSignsIn("100", "Sliva", "abc", ("g1", "Guild One", "icon1", "8"), ("g2", "Guild Two", null, "0"));

            var answer = await Anonymous.Get("/api/authorizeByCode?authorizationCode=code-1");

            var user = answer.Ok();
            Assert.Equal("100", user.Str("id"));
            Assert.Equal("Sliva", user.Str("userName"));
            Assert.Equal($"{ApiFactory.DiscordImages}avatars/100/abc.jpg", user.Str("avatarUrl"));
            var cookie = answer.Headers["Set-Cookie"];
            Assert.StartsWith("application_token=", cookie);
            Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("samesite=none", cookie, StringComparison.OrdinalIgnoreCase);
            // the code is exchanged with the client of the site
            var tokenRequest = External.Requests.Single(r => r.Address.EndsWith("/oauth2/token"));
            Assert.Contains("grant_type=authorization_code", tokenRequest.Body);
            Assert.Contains("code=code-1", tokenRequest.Body);
            Assert.Equal("Basic dGVzdC1kaXNjb3JkLWNsaWVudDp0ZXN0LWRpc2NvcmQtc2VjcmV0", tokenRequest.Headers["Authorization"]);

            var memberships = await Db(db => db.UserGuilds.Where(ug => ug.UserId == "100").OrderBy(ug => ug.GuildId).ToListAsync());
            Assert.Equal(new[] { ("g1", true), ("g2", false) }, memberships.Select(m => (m.GuildId, m.IsAdmin)));
            var guilds = await Db(db => db.Guilds.OrderBy(g => g.Id).ToListAsync());
            Assert.Equal(new[] { $"{ApiFactory.DiscordImages}icons/g1/icon1.jpg", null }, guilds.Select(g => g.AvatarUrl));
        }

        [Fact]
        public async Task SignIn_AnimatedAvatar_IsAGif()
        {
            DiscordSignsIn("100", "Sliva", "a_anim");

            var user = (await Anonymous.Get("/api/authorizeByCode?authorizationCode=c")).Ok();

            Assert.Equal($"{ApiFactory.DiscordImages}avatars/100/a_anim.gif", user.Str("avatarUrl"));
        }

        [Fact]
        public async Task SignIn_TheCookieOpensTheSite()
        {
            DiscordSignsIn("100", "Sliva", "abc");
            var answer = await Anonymous.Get("/api/authorizeByCode?authorizationCode=c");
            var token = answer.Headers["Set-Cookie"].Split(';')[0]["application_token=".Length..];

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/getUserObject");
            request.Headers.Add("Cookie", $"application_token={token}");
            var user = (await Anonymous.Send(request)).Ok();

            Assert.Equal("Sliva", user.Str("userName"));
        }

        [Fact]
        public async Task SignInAgain_UpdatesTheUser_MarksTheGuildsTheyLeft_AndAddsNewOnes()
        {
            DiscordSignsIn("100", "Sliva", "abc", ("g1", "Guild One", null, "8"));
            (await Anonymous.Get("/api/authorizeByCode?authorizationCode=c")).Ok();
            External.Reset();
            // 2147483647 has the bit of the administrator (0x8)
            DiscordSignsIn("100", "Sliva Renamed", "def", ("g3", "Guild Three", null, "2147483647"));

            (await Anonymous.Get("/api/authorizeByCode?authorizationCode=c2")).Ok();

            var user = await Db(db => db.Users.SingleAsync(u => u.Id == "100"));
            Assert.Equal("Sliva Renamed", user.UserName);
            Assert.Equal($"{ApiFactory.DiscordImages}avatars/100/def.jpg", user.AvatarUrl);
            var memberships = await Db(db => db.UserGuilds.Where(ug => ug.UserId == "100").OrderBy(ug => ug.GuildId).ToListAsync());
            Assert.Equal(new[] { ("g1", true, true), ("g3", true, false) }, memberships.Select(m => (m.GuildId, m.IsAdmin, m.IsDeleted)));
        }

        [Fact]
        public async Task SignInAgain_WithAGuildThatIsSaved_UpdatesIt_AndKeepsItsEventsAndMemberships()
        {
            // the sign-in used to remove the guild and add it again, and EF deleted in cascade what it had loaded with the guild:
            // the memberships of every player of the guild, its events and their records
            var createdAt = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero);
            var guild = Make.Guild("g2");
            guild.CreatedAt = createdAt;
            await Seed(Make.User("100", "A"), Make.User("200", "B"), guild, Make.Member("100", "g2"), Make.Member("200", "g2", isAdmin: true),
                Make.Event("e1", "g2", "200"), Make.Record("r1", "e1", "200"));
            DiscordSignsIn("100", "A", "x", ("g2", "G2", "icon2", "8"));

            (await Anonymous.Get("/api/authorizeByCode?authorizationCode=c")).Ok();
            (await Anonymous.Get("/api/authorizeByCode?authorizationCode=c")).Ok();

            var memberships = await Db(db => db.UserGuilds.OrderBy(ug => ug.UserId).ToListAsync());
            Assert.Equal(new[] { ("100", "g2", true, false), ("200", "g2", true, false) },
                memberships.Select(m => (m.UserId, m.GuildId, m.IsAdmin, m.IsDeleted)));
            Assert.Equal(1, await Factory.ScalarAsync("SELECT COUNT(*) FROM Events"));
            Assert.Equal(1, await Factory.ScalarAsync("SELECT COUNT(*) FROM EventRecords"));
            var saved = await Db(db => db.Guilds.SingleAsync(g => g.Id == "g2"));
            Assert.Equal(("G2", $"{ApiFactory.DiscordImages}icons/g2/icon2.jpg", (DateTimeOffset?)createdAt), (saved.Name, saved.AvatarUrl, saved.CreatedAt));
        }

        [Fact]
        public async Task GuildSearch_FindsTheGuild_AfterEverySignIn()
        {
            // the sign-in used to delete the memberships of the guilds the user was already in, so every other sign-in left the search empty
            await Seed(Make.User("200", "B"), Make.Guild("g2", "Guild Two"), Make.Member("200", "g2"));
            DiscordSignsIn("100", "A", "x", ("g2", "Guild Two", null, "0"));
            var found = new List<(int SignIn, string Who, string[] Guilds)>();

            for (var signIn = 1; signIn <= 3; signIn++)
            {
                (await Anonymous.Get("/api/authorizeByCode?authorizationCode=c")).Ok();
                foreach (var who in new[] { "100", "200" })
                {
                    found.Add((signIn, who, (await As(who).Get("/api/getSearchedGuilds?searchString=two")).Ok().Strs("guildObjects", "id").ToArray()));
                }
            }

            Assert.All(found, f => Assert.Equal(new[] { "g2" }, f.Guilds));
        }

        [Fact]
        public async Task SignIn_ANewMemberOfASavedGuild_KeepsItsEvents_AndItsCreationDate()
        {
            var createdAt = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.Zero);
            var guild = Make.Guild("g2");
            guild.CreatedAt = createdAt;
            await Seed(Make.User("200", "B"), guild, Make.Member("200", "g2", isAdmin: true),
                Make.Event("e1", "g2", "200"), Make.Record("r1", "e1", "200"));
            DiscordSignsIn("100", "A", "x", ("g2", "G2", null, "0"));

            (await Anonymous.Get("/api/authorizeByCode?authorizationCode=c")).Ok();

            var memberships = await Db(db => db.UserGuilds.OrderBy(ug => ug.UserId).ToListAsync());
            Assert.Equal(new[] { ("100", false), ("200", true) }, memberships.Select(m => (m.UserId, m.IsAdmin)));
            Assert.Equal(1, await Factory.ScalarAsync("SELECT COUNT(*) FROM Events"));
            Assert.Equal(1, await Factory.ScalarAsync("SELECT COUNT(*) FROM EventRecords"));
            // the guild was written again whole, its creation date was erased
            Assert.Equal(createdAt, (await Db(db => db.Guilds.SingleAsync(g => g.Id == "g2"))).CreatedAt);
        }

        [Fact]
        [Trait("Bug", "Users without an avatar can't sign in")]
        public async Task SignIn_UserWithoutAvatar_Fails()
        {
            // the avatar column is required, a Discord account without an avatar can't be saved (current behavior)
            DiscordSignsIn("100", "NoAvatar", null);

            var answer = await Anonymous.Get("/api/authorizeByCode?authorizationCode=c");

            answer.Is(HttpStatusCode.InternalServerError);
            Assert.IsType<DbUpdateException>(Assert.Single(Factory.UnhandledExceptions));
            Assert.Equal(0, await Factory.ScalarAsync("SELECT COUNT(*) FROM Users"));
        }

        [Fact]
        public async Task UserObject_GivesTheProfileOfTheSignedInUser()
        {
            var user = Make.User("100", "Sliva", isAppAdmin: true);
            user.SteamId = "76561198000000001";
            user.XboxGamertag = "SlivaXbox";
            user.PsnOnlineId = "SlivaPsn";
            await Seed(user);

            var answer = (await As("100").Get("/api/getUserObject")).Ok();

            Assert.Equal("100", answer.Str("id"));
            Assert.Equal("Sliva", answer.Str("userName"));
            Assert.Equal("100@mail.test", answer.Str("email"));
            Assert.True(answer.Bool("isAppAdmin"));
            Assert.Equal("76561198000000001", answer.Str("steamId"));
            Assert.Equal("SlivaXbox", answer.Str("xboxGamertag"));
            Assert.Equal("SlivaPsn", answer.Str("psnOnlineId"));
        }

        [Fact]
        public async Task UserObject_OfAnUnknownUser_IsEmpty()
        {
            var answer = await As("ghost").Get("/api/getUserObject");

            answer.Is(HttpStatusCode.OK);
            Assert.Equal("", answer.Body);
        }

        [Fact]
        public async Task Token_Expired_IsRefused()
        {
            await Seed(Make.User("100"));
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/getUserObject");
            request.Headers.Add("Cookie", $"application_token={Token("100", DateTime.UtcNow.AddMinutes(-10))}");

            (await Anonymous.Send(request)).Is(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Token_SignedByAnotherSecret_IsRefused()
        {
            await Seed(Make.User("100"));
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/getUserObject");
            request.Headers.Add("Cookie", $"application_token={Token("100", secret: "another-secret-that-is-long-enough-for-hmac-sha256-too")}");

            (await Anonymous.Send(request)).Is(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Logout_RemovesTheCookie_EvenWithoutSignIn()
        {
            var answer = await Anonymous.Post("/api/logout");

            answer.Is(HttpStatusCode.OK);
            Assert.StartsWith("application_token=;", answer.Headers["Set-Cookie"]);
            Assert.Contains("expires=Thu, 01 Jan 1970", answer.Headers["Set-Cookie"]);
        }

        [Fact]
        public async Task IsAuthorized_IsNotImplemented()
        {
            // the endpoint throws NotImplementedException (current behavior), the front doesn't call it
            (await As("100").Get("/api/getIsAuthorized")).Is(HttpStatusCode.InternalServerError);
            Assert.IsType<NotImplementedException>(Assert.Single(Factory.UnhandledExceptions));
        }

        [Theory]
        [InlineData("GET", "/api/getUserObject")]
        [InlineData("GET", "/api/getEventSeasons")]
        [InlineData("GET", "/api/getPlayedGuilds")]
        [InlineData("GET", "/api/getActiveGlobalEvent")]
        [InlineData("GET", "/api/getPreferenceTags")]
        [InlineData("GET", "/api/getCommunityReviews?count=5")]
        [InlineData("POST", "/api/postNewEvent")]
        [InlineData("POST", "/api/joinEvent")]
        [InlineData("POST", "/api/publishReview")]
        [InlineData("POST", "/api/rollGlobalEventGames")]
        public async Task Endpoints_NeedASignedInUser(string method, string path)
        {
            var answer = method == "GET" ? await Anonymous.Get(path) : await Anonymous.Post(path, new { });

            answer.Is(HttpStatusCode.Unauthorized);
        }
    }
}
