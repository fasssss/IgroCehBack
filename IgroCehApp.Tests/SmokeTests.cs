using System.Net;
using IgroCehApp.Tests.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IgroCehApp.Tests
{
    // the stand of the tests itself: the app runs on its own settings and the database starts empty
    public class SmokeTests : ApiTest
    {
        public SmokeTests(ApiFactory factory) : base(factory)
        {
        }

        [Fact]
        public void App_RunsInTheTestingEnvironment_FromAnEmptyFolder()
        {
            var environment = Factory.Services.GetRequiredService<IWebHostEnvironment>();

            Assert.Equal("Testing", environment.EnvironmentName);
            Assert.Equal(Path.GetFullPath(Path.GetDirectoryName(Factory.UploadFolder)!), Path.GetFullPath(environment.ContentRootPath).TrimEnd('\\', '/'));
        }

        [Fact]
        public async Task Database_HasOnlyTheSeededRows()
        {
            Assert.Equal(0, await Factory.ScalarAsync("SELECT COUNT(*) FROM Users"));
            Assert.Equal(6, await Factory.ScalarAsync("SELECT COUNT(*) FROM EventStatuses"));
            Assert.True(await Factory.ScalarAsync("SELECT COUNT(*) FROM PreferenceTags") > 0);
            Assert.Equal(2, await Factory.ScalarAsync("SELECT COUNT(*) FROM Badges"));
        }

        [Fact]
        public async Task SignedInRequest_WithoutCookie_Is401()
        {
            (await Anonymous.Get("/api/getUserObject")).Is(HttpStatusCode.Unauthorized);
        }
    }
}
