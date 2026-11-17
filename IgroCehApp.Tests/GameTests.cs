using System.Net;
using System.Net.Http.Headers;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace IgroCehApp.Tests
{
    // the games players guess: the ones added by hand with a cover, and the ones of the VideoGamesCritic catalog
    public class GameTests : GuildTest
    {
        private const string SearchUrl = ApiFactory.CatalogAddress + "/api/v1/search";

        public GameTests(ApiFactory factory) : base(factory)
        {
        }

        private static object CatalogGame(int id, string name, string? cover = null, string? steam = null, params string[] platforms) => new
        {
            id,
            slug = name.ToLowerInvariant().Replace(' ', '-'),
            name,
            url = $"https://vgc.test/game/{id}",
            releaseDate = "2017-02-24",
            scores = new { vgc = 90.4, players = 91.6, critics = 87.5 },
            platforms,
            art = cover == null ? null : new { cover, header = (string?)null },
            links = steam == null ? null : new { page = $"https://vgc.test/page/{id}", steam },
        };

        private MultipartFormDataContent GameForm(string name, byte[]? image = null, string contentType = "image/png")
        {
            var form = new MultipartFormDataContent { { new StringContent(name), "gameName" } };
            var file = new ByteArrayContent(image ?? Array.Empty<byte>());
            file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            form.Add(file, "image", "cover.png");
            return form;
        }

        [Fact]
        public async Task FindByName_GivesTheLatestGameWithExactlyThisName()
        {
            await Seed(Make.Game("old", Sliva, "Doom"), Make.Game("doom-eternal", Sliva, "Doom Eternal"));
            await Db(async db =>
            {
                db.Games.Find("old")!.CreatedAt = DateTimeOffset.UtcNow.AddDays(-1);
                await db.SaveChangesAsync();
            });
            await Seed(Make.Game("new", Sliva, "Doom", imageUrl: "/Upload/doom.png"));

            var found = (await As(Dayt).Get("/api/findGameByName?name=Doom")).Ok().At("gameObject");
            var missing = (await As(Dayt).Get("/api/findGameByName?name=Quake")).Ok();

            Assert.Equal(("new", "Doom", "/Upload/doom.png"), (found.Str("id"), found.Str("name"), found.Str("imageUrl")));
            Assert.Null(missing.At("gameObject"));
        }

        [Fact]
        public async Task Create_ByHand_WithoutACover()
        {
            var gameId = (await As(Dayt).PostForm("/api/createGame", GameForm("My Game"))).Ok().Str("gameId");

            var game = await Db(db => db.Games.SingleAsync(g => g.Id == gameId));
            Assert.Equal(("My Game", Dayt, null, null), (game.Name, game.CreatorId, game.ImageUrl, game.VgcId));
            Assert.Empty(game.Platforms);
        }

        [Fact]
        public async Task Create_ByHand_TheCoverIsSavedAndServed()
        {
            var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 1, 2, 3 };

            var gameId = (await As(Dayt).PostForm("/api/createGame", GameForm("Covered", png))).Ok().Str("gameId");

            var imageUrl = (await Db(db => db.Games.SingleAsync(g => g.Id == gameId))).ImageUrl!;
            Assert.Matches($"^/Upload/{Dayt}/Images/Games/[0-9a-f]{{32}}\\.png$", imageUrl);
            var served = await Anonymous.Get(imageUrl);
            served.Is(HttpStatusCode.OK);
            Assert.Equal(png, served.Bytes);
        }

        [Theory]
        [InlineData("image/svg+xml")]
        [InlineData("text/plain")]
        public async Task Create_ByHand_ACoverOfAnotherType_IsRefused(string contentType)
        {
            var answer = await As(Dayt).PostForm("/api/createGame", GameForm("Bad", new byte[] { 1, 2, 3 }, contentType));

            answer.Is(HttpStatusCode.BadRequest);
            Assert.Contains("Image must be PNG, JPEG, WebP or GIF", answer.Body);
            Assert.Equal(0, await Factory.ScalarAsync("SELECT COUNT(*) FROM Games WHERE Name = 'Bad'"));
        }

        [Fact]
        public async Task Create_ByHand_ACoverOver10Mb_IsRefused()
        {
            var answer = await As(Dayt).PostForm("/api/createGame", GameForm("Huge", new byte[10 * 1024 * 1024 + 1]));

            answer.Is(HttpStatusCode.BadRequest);
            Assert.Contains("Image must be up to 10 MB", answer.Body);
        }

        [Fact]
        public async Task Search_InTheCatalog()
        {
            External.OnGet(SearchUrl, new
            {
                query = "hollow knight",
                total = 2,
                games = new[]
                {
                    CatalogGame(367520, "Hollow Knight", "https://cdn.steam.test/367520.jpg", null, "PC", "Switch"),
                    CatalogGame(1000012522, "Ninja Gaiden 3", "/api/img?u=cover&w=480", null, "PS3"),
                },
            });

            var games = (await As(Dayt).Get("/api/searchGames?query=%20Hollow%20%20Knight%20")).Ok().Arr("games");

            Assert.Equal("?q=hollow%20knight&limit=10", External.Requests.Single().Query);
            Assert.Equal(new[] { 367520, 1000012522 }, games.Select(g => g.Int("vgcId")));
            var hollow = games[0];
            Assert.Equal(("Hollow Knight", "2017-02-24", "https://cdn.steam.test/367520.jpg"), (hollow.Str("name"), hollow.Str("releaseDate"), hollow.Str("coverUrl")));
            Assert.Equal("https://store.steampowered.com/app/367520/", hollow.Str("steamUrl"));
            Assert.Equal("https://vgc.test/game/367520", hollow.Str("pageUrl"));
            Assert.Equal(new[] { "PC", "Switch" }, hollow.Strs("platforms"));
            Assert.Equal((88, 92), (hollow.Int("criticsScore"), hollow.Int("playersScore")));
            // a game outside Steam has no store link, its cover comes through the image proxy of the catalog
            Assert.Null(games[1].Str("steamUrl"));
            Assert.Equal("https://vgc.test/api/img?u=cover&w=480", games[1].Str("coverUrl"));
        }

        [Fact]
        public async Task Search_ShortQueries_DontReachTheCatalog_AndAnswersAreCached()
        {
            External.OnGet(SearchUrl, new { query = "doom", total = 0, games = Array.Empty<object>() });

            Assert.Empty((await As(Dayt).Get("/api/searchGames?query=d")).Ok().Arr("games"));
            (await As(Dayt).Get("/api/searchGames?query=Doom")).Ok();
            (await As(Nico).Get("/api/searchGames?query=doom")).Ok();

            Assert.Equal(1, External.CountOf(SearchUrl));
        }

        [Fact]
        public async Task Search_WhenTheCatalogFails_Is503()
        {
            External.OnGet(SearchUrl, new { error = "down" }, HttpStatusCode.InternalServerError);

            (await As(Dayt).Get("/api/searchGames?query=doom")).Is(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task CreateFromCatalog_SavesTheGameOnce_AndReusesIt()
        {
            External.OnGet($"{ApiFactory.CatalogAddress}/api/v1/game/367520",
                CatalogGame(367520, "Hollow Knight", "https://cdn.steam.test/367520.jpg", "https://store.steampowered.com/app/367520/hk/", "PC"));

            var first = (await As(Dayt).Post("/api/createGameFromVgc", new { vgcId = 367520 })).Ok().Str("gameId");
            var second = (await As(Nico).Post("/api/createGameFromVgc", new { vgcId = 367520 })).Ok().Str("gameId");

            Assert.Equal(first, second);
            var game = await Db(db => db.Games.SingleAsync(g => g.VgcId == 367520));
            Assert.Equal(("Hollow Knight", "https://cdn.steam.test/367520.jpg", "https://store.steampowered.com/app/367520/hk/", Dayt),
                (game.Name, game.ImageUrl, game.SteamUrl, game.CreatorId));
            Assert.Equal(new[] { "PC" }, game.Platforms);
            Assert.Equal(1, External.CountOf($"{ApiFactory.CatalogAddress}/api/v1/game/367520"));
        }

        [Fact]
        public async Task CreateFromCatalog_AGameFoundBySearch_IsSavedWithoutAnotherRequest()
        {
            External.OnGet(SearchUrl, new { query = "sekiro", total = 1, games = new[] { CatalogGame(814380, "Sekiro", null, null, "PC") } });
            (await As(Dayt).Get("/api/searchGames?query=sekiro")).Ok();

            (await As(Dayt).Post("/api/createGameFromVgc", new { vgcId = 814380 })).Ok();

            Assert.Equal(0, External.CountOf($"{ApiFactory.CatalogAddress}/api/v1/game/814380"));
            Assert.Equal("https://store.steampowered.com/app/814380/", (await Db(db => db.Games.SingleAsync(g => g.VgcId == 814380))).SteamUrl);
        }

        [Fact]
        public async Task CreateFromCatalog_AnOldGameWithoutPlatforms_GetsThem()
        {
            await Seed(Make.Game("old-hollow", Sliva, "Hollow Knight", vgcId: 367520));
            External.OnGet($"{ApiFactory.CatalogAddress}/api/v1/game/367520", CatalogGame(367520, "Hollow Knight", null, null, "PC", "Switch"));

            var gameId = (await As(Dayt).Post("/api/createGameFromVgc", new { vgcId = 367520 })).Ok().Str("gameId");

            Assert.Equal("old-hollow", gameId);
            Assert.Equal(new[] { "PC", "Switch" }, (await Db(db => db.Games.SingleAsync(g => g.Id == "old-hollow"))).Platforms);
        }

        [Fact]
        public async Task CreateFromCatalog_AnUnknownGame_IsNotFound_AndAFailingCatalog_Is503()
        {
            External.OnGet($"{ApiFactory.CatalogAddress}/api/v1/game/1", new { error = "not found" }, HttpStatusCode.NotFound);
            External.OnGet($"{ApiFactory.CatalogAddress}/api/v1/game/2", new { error = "down" }, HttpStatusCode.BadGateway);

            (await As(Dayt).Post("/api/createGameFromVgc", new { vgcId = 1 })).Is(HttpStatusCode.NotFound);
            (await As(Dayt).Post("/api/createGameFromVgc", new { vgcId = 2 })).Is(HttpStatusCode.ServiceUnavailable);
        }

        [Fact]
        public async Task Catalog_IsAskedWithTheKey()
        {
            External.OnGet(SearchUrl, new { query = "doom", total = 0, games = Array.Empty<object>() });

            (await As(Dayt).Get("/api/searchGames?query=doom")).Ok();

            Assert.Equal("Bearer test-vgc-key", External.Requests.Single().Headers["Authorization"]);
        }
    }
}
