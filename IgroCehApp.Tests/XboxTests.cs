using System.Net;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // Xbox through OpenXBL: a player links their account, the administrator looks for the games and their achievements
    public class XboxTests : ApiTest
    {
        private const string ClaimUrl = ApiFactory.XboxAddress + "/app/claim";

        public XboxTests(ApiFactory factory) : base(factory)
        {
        }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await Seed(Make.User("dayt", "Dayt"), Make.User("boss", "Boss", isAppAdmin: true));
        }

        private static object Achievement(string id, string name, string description, double percent, bool isSecret = false) => new
        {
            id,
            name,
            description,
            lockedDescription = "Locked",
            isSecret,
            progressState = "NotStarted",
            mediaAssets = new[] { new { type = "Icon", url = $"https://xbl.test/icons/{id}.png" } },
            rarity = new { currentPercentage = percent },
            titleAssociations = new[] { new { name = "Halo Infinite", id = 2043073184L } },
        };

        [Fact]
        public async Task SignInLink_IsThePageOfTheAppOnOpenXbl()
        {
            Assert.Equal($"{ApiFactory.XboxAddress}/app/auth/{ApiFactory.XboxPublicKey}",
                (await As("dayt").Get("/api/getXboxSignInLink")).Ok().Str("xboxSignInLink"));
        }

        [Fact]
        public async Task Link_TheCodeIsClaimed_TheAccountIsSaved()
        {
            External.OnPost(ClaimUrl, new { xuid = "2535400000000001", gamertag = "DaytXbox" });

            Assert.Equal("DaytXbox", (await As("dayt").Post("/api/linkXbox", new { code = "code-1" })).Ok().Str("xboxGamertag"));

            var user = await Db(db => db.Users.SingleAsync(u => u.Id == "dayt"));
            Assert.Equal(("2535400000000001", "DaytXbox"), (user.XboxXuid, user.XboxGamertag));
            var claim = External.Requests.Single();
            Assert.Contains("\"code\":\"code-1\"", claim.Body);
            Assert.Contains($"\"app_key\":\"{ApiFactory.XboxPublicKey}\"", claim.Body);
        }

        [Fact]
        public async Task Link_IsRefused()
        {
            External.OnPost(ClaimUrl, new { error = "bad code" }, HttpStatusCode.BadRequest);
            Assert.Equal("Xbox doesn't confirm the sign-in", (await As("dayt").Post("/api/linkXbox", new { code = "bad" })).Is(HttpStatusCode.BadRequest).Text);

            External.OnPost(ClaimUrl, new { xuid = "not-a-number", gamertag = "X" });
            (await As("dayt").Post("/api/linkXbox", new { code = "c" })).Is(HttpStatusCode.BadRequest);

            External.OnPost(ClaimUrl, new { }, HttpStatusCode.InternalServerError);
            (await As("dayt").Post("/api/linkXbox", new { code = "c" })).Is(HttpStatusCode.ServiceUnavailable);

            (await As("dayt").Post("/api/linkXbox", new { code = "" })).Is(HttpStatusCode.BadRequest);
            Assert.Null((await Db(db => db.Users.SingleAsync(u => u.Id == "dayt"))).XboxXuid);
        }

        [Fact]
        public async Task Unlink()
        {
            await Db(async db =>
            {
                var user = db.Users.Find("dayt")!;
                user.XboxXuid = "1";
                user.XboxGamertag = "X";
                await db.SaveChangesAsync();
            });

            (await As("dayt").Post("/api/unlinkXbox")).Ok();

            var saved = await Db(db => db.Users.SingleAsync(u => u.Id == "dayt"));
            Assert.Equal((null, null), (saved.XboxXuid, saved.XboxGamertag));
        }

        [Fact]
        public async Task Search_TheGamesOfTheStoreStartingWithTheQuery()
        {
            External.OnGet($"{ApiFactory.XboxAddress}/v2/marketplace/autosuggest", new
            {
                content = new
                {
                    results = new[]
                    {
                        new
                        {
                            products = new[]
                            {
                                new { productId = "9PP5G1F0C2B6", title = "Halo Infinite", type = "Game", icon = "//store-images.test/halo.png" },
                                new { productId = "9NBLGGH4R315", title = "Halo Wars", type = "Game", icon = "https://store-images.test/wars.png" },
                                new { productId = "CFQ7TTC0KHS0", title = "Halo Bundle", type = "Bundle", icon = "" },
                            },
                        },
                    },
                },
            });

            var titles = (await As("boss").Get("/api/searchXboxTitles?query=Halo")).Ok().Arr("titles");

            Assert.Equal(new[] { ("9PP5G1F0C2B6", "Halo Infinite", "https://store-images.test/halo.png"), ("9NBLGGH4R315", "Halo Wars", "https://store-images.test/wars.png") },
                titles.Select(t => (t.Str("productId"), t.Str("name"), t.Str("imageUrl"))));
            Assert.All(titles, t => Assert.Null(t.At("titleId")));
            Assert.Equal("test-xbox-key", External.Requests.Single().Headers["X-Authorization"]);
        }

        [Fact]
        public async Task TitleId_OfAProduct()
        {
            External.OnGet($"{ApiFactory.XboxAddress}/v2/marketplace/titleid/9PP5G1F0C2B6", new { content = new { productId = "9PP5G1F0C2B6", titleId = 2043073184L } });
            External.OnGet($"{ApiFactory.XboxAddress}/v2/marketplace/titleid/CFQ7TTC0KHS0", new { code = 404 }, HttpStatusCode.NotFound);

            Assert.Equal("2043073184", (await As("boss").Get("/api/getXboxTitleId?productId=9PP5G1F0C2B6")).Ok().Str("titleId"));
            (await As("boss").Get("/api/getXboxTitleId?productId=CFQ7TTC0KHS0")).Is(HttpStatusCode.NotFound);
            (await As("boss").Get("/api/getXboxTitleId?productId=bad-id")).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Achievements_OfATitle_InBothLanguages()
        {
            External.On(HttpMethod.Get, $"{ApiFactory.XboxAddress}/v2/achievements/title/2043073184", (request, _) =>
            {
                var isRussian = request.Headers.AcceptLanguage.ToString() == "ru-RU";
                return FakeExternalApis.Json(new
                {
                    content = new
                    {
                        achievements = new[]
                        {
                            Achievement("10", isRussian ? "Десять" : "Ten", isRussian ? "" : "Ten desc", 12.34),
                            Achievement("2", isRussian ? "Два" : "Two", isRussian ? "Два описание" : "Two desc", 50, isSecret: true),
                        },
                        pagingInfo = new { continuationToken = (string?)null },
                    },
                });
            });

            var answer = (await As("boss").Get("/api/getXboxAchievements?titleId=2043073184")).Ok();

            Assert.Equal(("2043073184", "Halo Infinite"), (answer.Str("title.titleId"), answer.Str("title.name")));
            var achievements = answer.Arr("achievements");
            Assert.Equal(new[] { "2", "10" }, achievements.Select(a => a.Str("apiName")));
            Assert.Equal(("Two", "Два", "Two desc", "Два описание", true, 50.0), (achievements[0].Str("name"), achievements[0].Str("nameRu"),
                achievements[0].Str("description"), achievements[0].Str("descriptionRu"), achievements[0].Bool("isHidden"), achievements[0].Num("percent")!.Value));
            // the empty Russian description falls back to the locked one
            Assert.Equal(("Десять", "Locked", 12.3), (achievements[1].Str("nameRu"), achievements[1].Str("descriptionRu"), achievements[1].Num("percent")!.Value));
            Assert.Equal("https://xbl.test/icons/10.png", achievements[1].Str("iconUrl"));
        }

        [Fact]
        public async Task Achievements_OfAnUnknownTitle_AreNotFound()
        {
            External.OnGet($"{ApiFactory.XboxAddress}/v2/achievements/title/123", new { content = new { achievements = Array.Empty<object>() } });

            (await As("boss").Get("/api/getXboxAchievements?titleId=123")).Is(HttpStatusCode.NotFound);
            (await As("boss").Get("/api/getXboxAchievements?titleId=abc")).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task AdminEndpoints_AreForTheAdministratorOnly()
        {
            (await As("dayt").Get("/api/searchXboxTitles?query=Halo")).Is(HttpStatusCode.Forbidden);
            (await As("dayt").Get("/api/getXboxTitleId?productId=9PP5G1F0C2B6")).Is(HttpStatusCode.Forbidden);
            (await As("dayt").Get("/api/getXboxAchievements?titleId=1")).Is(HttpStatusCode.Forbidden);
        }
    }
}
