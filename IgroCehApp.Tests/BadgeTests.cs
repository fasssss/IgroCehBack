using System.Net;
using System.Net.Http.Headers;
using System.Text;
using IgroCehApp.Tests.Infrastructure;

namespace IgroCehApp.Tests
{
    // the badges the administrator uploads for passing global events: only safe images are taken, everyone may load them
    public class BadgeTests : GlobalEventTest
    {
        private const string SafeSvg = "<svg xmlns=\"http://www.w3.org/2000/svg\" viewBox=\"0 0 10 10\"><circle cx=\"5\" cy=\"5\" r=\"4\" fill=\"gold\"/>"
            + "<use href=\"#a\"/></svg>";

        public BadgeTests(ApiFactory factory) : base(factory)
        {
        }

        private Task<ApiResult> Upload(string name, byte[] content, string contentType, string by = Boss)
        {
            var file = new ByteArrayContent(content);
            file.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            return As(by).PostForm("/api/uploadBadge", new MultipartFormDataContent { { new StringContent(name), "name" }, { file, "image", "badge" } });
        }

        [Fact]
        public async Task List_TheSeededBadges_WithTheEventsThatUseThem()
        {
            var before = (await As(Boss).Get("/api/getBadges")).Ok().Arr("badges");
            var id = await CreateGlobalEvent();
            var after = (await As(Boss).Get("/api/getBadges")).Ok().Arr("badges");

            Assert.Equal(new[] { ("knight-badge", "Рыцарь"), ("wheel-badge", "Колесо") }, before.Select(b => (b.Str("id"), b.Str("name"))));
            Assert.All(before, b => Assert.Empty(b.Arr("uses")));
            var use = Assert.Single(after[0].Arr("uses"));
            Assert.Equal((id, "Ladder", 0), (use.Str("globalEventId"), use.Str("globalEventName"), use.Int("passedCount")));
        }

        [Fact]
        public async Task Upload_ASafeSvg_IsServedToEveryone()
        {
            var badge = (await Upload("  Gold  ", Encoding.UTF8.GetBytes(SafeSvg), "image/svg+xml")).Ok().At("badge");

            Assert.Equal("Gold", badge.Str("name"));
            Assert.Empty(badge.Arr("uses"));
            var image = await Anonymous.Get($"/api/getBadgeImage?badgeId={badge.Str("id")}");
            image.Is(HttpStatusCode.OK);
            Assert.Equal("image/svg+xml", image.ContentType);
            Assert.Equal(SafeSvg, Encoding.UTF8.GetString(image.Bytes));
            Assert.Equal("public, max-age=31536000, immutable", image.Headers["Cache-Control"]);
            Assert.Equal("default-src 'none'; style-src 'unsafe-inline'; img-src data:", image.Headers["Content-Security-Policy"]);
            Assert.Equal("nosniff", image.Headers["X-Content-Type-Options"]);
            Assert.Equal(3, (await As(Boss).Get("/api/getBadges")).Ok().Arr("badges").Count);
        }

        [Theory]
        [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\"><script>alert(1)</script></svg>")]
        [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\" onload=\"alert(1)\"></svg>")]
        [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\"><foreignObject/></svg>")]
        [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\"><image href=\"https://evil.test/x.png\"/></svg>")]
        [InlineData("<svg xmlns=\"http://www.w3.org/2000/svg\"><a fill=\"java script:alert(1)\"/></svg>")]
        [InlineData("<!DOCTYPE svg [<!ENTITY x \"y\">]><svg xmlns=\"http://www.w3.org/2000/svg\">&x;</svg>")]
        [InlineData("<html xmlns=\"http://www.w3.org/1999/xhtml\"></html>")]
        [InlineData("not xml")]
        public async Task Upload_AnUnsafeSvg_IsRefused(string svg)
        {
            var answer = await Upload("Bad", Encoding.UTF8.GetBytes(svg), "image/svg+xml");

            Assert.Equal("The image isn't a safe SVG, a PNG or a WebP", answer.Is(HttpStatusCode.BadRequest).Text);
            Assert.Equal(2, await Factory.ScalarAsync("SELECT COUNT(*) FROM Badges"));
        }

        [Fact]
        public async Task Upload_PngAndWebp_AreCheckedByTheirSignatures()
        {
            var png = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0 };
            var webp = Encoding.ASCII.GetBytes("RIFF\0\0\0\0WEBPVP8 ");

            (await Upload("Png", png, "image/png")).Ok();
            (await Upload("Webp", webp, "image/webp")).Ok();
            (await Upload("Fake png", webp, "image/png")).Is(HttpStatusCode.BadRequest);
            (await Upload("Fake webp", png, "image/webp")).Is(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Upload_IsValidated()
        {
            Assert.Contains("Image must be SVG, PNG or WebP", (await Upload("Jpeg", new byte[] { 1 }, "image/jpeg")).Is(HttpStatusCode.BadRequest).Body);
            Assert.Contains("Image must be up to 256 KB", (await Upload("Huge", new byte[256 * 1024 + 1], "image/png")).Is(HttpStatusCode.BadRequest).Body);
            Assert.Contains("The badge needs a name", (await Upload(" ", Encoding.UTF8.GetBytes(SafeSvg), "image/svg+xml")).Is(HttpStatusCode.BadRequest).Body);
            Assert.Contains("The image is empty", (await Upload("Empty", Array.Empty<byte>(), "image/png")).Is(HttpStatusCode.BadRequest).Body);
            (await Upload(new string('n', 51), Encoding.UTF8.GetBytes(SafeSvg), "image/svg+xml")).Is(HttpStatusCode.BadRequest);
            (await Upload("Mine", Encoding.UTF8.GetBytes(SafeSvg), "image/svg+xml", by: Dayt)).Is(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task Image_OfAnUnknownBadge_IsNotFound()
        {
            (await Anonymous.Get("/api/getBadgeImage?badgeId=unknown")).Is(HttpStatusCode.NotFound);
            Assert.Equal("image/svg+xml", (await Anonymous.Get($"/api/getBadgeImage?badgeId={KnightBadge}")).Is(HttpStatusCode.OK).ContentType);
        }
    }
}
