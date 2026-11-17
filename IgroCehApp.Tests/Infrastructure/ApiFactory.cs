using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Logging;
using System.Reflection;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Infrastructure.Services;
using MySqlConnector;
using Persistence.Context;

namespace IgroCehApp.Tests.Infrastructure
{
    // the whole API in memory against a real MySQL in Docker, the same server as in production;
    // the settings come only from here: the content root is an empty folder, so appsettings*.json and the user secrets aren't read
    public class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
    {
        public const string JwtSecret = "igroceh-api-tests-secret-that-is-long-enough-for-hmac-sha256";
        public const string JwtIssuer = "igroceh-api-tests";
        public const string JwtAudience = "igroceh-api-tests";
        public const string FrontendUrl = "https://front.test";
        public const string DiscordAddress = "https://discord.test";
        public const string DiscordImages = "https://cdn.discord.test/";
        public const string CatalogAddress = "https://vgc.test";
        public const string SteamAddress = "https://steam-api.test";
        public const string SteamCommunityAddress = "https://steamcommunity.test";
        public const string SteamKey = "test-steam-key";
        public const string XboxAddress = "https://xbl.test";
        public const string XboxPublicKey = "test-xbox-public-key";
        public const string XboxKey = "test-xbox-key";
        public const string PsnAuthAddress = "https://psn-auth.test";
        public const string PsnApiAddress = "https://psn-api.test";
        public const string PsnProfileAddress = "https://psn-profile.test";
        public const string PsnRedirectUri = "com.scee.psxandroid.scecompcall://redirect";

        private const int MySqlPort = 3306;
        private const string MySqlPassword = "igroceh-tests";
        private const string Database = "IgroCehTests";

        // the server of production is mysql/mysql-server; the MySQL module of Testcontainers writes a client config that breaks
        // the init of this image, so it is an ordinary container that is ready once the server listens on its port after the init
        private readonly IContainer _mySql = new ContainerBuilder()
            .WithImage("mysql/mysql-server:latest")
            .WithEnvironment("MYSQL_ROOT_PASSWORD", MySqlPassword)
            .WithEnvironment("MYSQL_ROOT_HOST", "%")
            .WithEnvironment("MYSQL_DATABASE", Database)
            .WithPortBinding(MySqlPort, true)
            .WithWaitStrategy(Wait.ForUnixContainer().UntilMessageIsLogged($"port: {MySqlPort}  MySQL Community Server"))
            .Build();
        private readonly string _contentRoot = Path.Combine(Path.GetTempPath(), "igroceh-api-tests-" + Guid.NewGuid().ToString("N"));
        private List<string> _tables = new();
        private List<string> _seededTables = new();

        public FakeExternalApis ExternalApis { get; } = new();

        public string ConnectionString =>
            $"Server={_mySql.Hostname};Port={_mySql.GetMappedPublicPort(MySqlPort)};Database={Database};Uid=root;Pwd={MySqlPassword};";

        public string UploadFolder => Path.Combine(_contentRoot, "Upload");

        public async Task InitializeAsync()
        {
            await _mySql.StartAsync();
            Directory.CreateDirectory(UploadFolder);
            // Program reads some settings before the host is built, so they come as environment variables
            foreach (var (key, value) in Settings(ConnectionString))
            {
                Environment.SetEnvironmentVariable(key, value);
            }

            // the first request starts the app, it applies the migrations
            _ = Server;
            await RememberSeedAsync();
        }

        public new async Task DisposeAsync()
        {
            await base.DisposeAsync();
            await _mySql.DisposeAsync();
            try
            {
                Directory.Delete(_contentRoot, true);
            }
            catch (IOException)
            {
                // a file of an upload may still be open, the folder is in the temp directory anyway
            }
        }

        public static IEnumerable<(string Key, string Value)> Settings(string connectionString) => new[]
        {
            ("ConnectionStrings__Default", connectionString),
            ("ApplicationJwtOptions__Secret", JwtSecret),
            ("ApplicationJwtOptions__Issuer", JwtIssuer),
            ("ApplicationJwtOptions__Audience", JwtAudience),
            ("ApplicationJwtOptions__ExpInHours", "24"),
            ("FrontendUrl", FrontendUrl),
            ("DiscordApiOptions__Address", DiscordAddress),
            ("DiscordApiOptions__ImagesBaseUrl", DiscordImages),
            ("DiscordApiOptions__ClientId", "test-discord-client"),
            ("DiscordApiOptions__ClientSecret", "test-discord-secret"),
            ("DiscordApiOptions__RedirectUri", FrontendUrl + "/auth"),
            ("VideoGamesCriticOptions__Address", CatalogAddress),
            ("VideoGamesCriticOptions__ApiKey", "test-vgc-key"),
            ("SteamApiOptions__Address", SteamAddress),
            ("SteamApiOptions__CommunityAddress", SteamCommunityAddress),
            ("SteamApiOptions__Key", SteamKey),
            ("XboxApiOptions__Address", XboxAddress),
            ("XboxApiOptions__PublicKey", XboxPublicKey),
            ("XboxApiOptions__Key", XboxKey),
            ("PsnApiOptions__AuthAddress", PsnAuthAddress),
            ("PsnApiOptions__ApiAddress", PsnApiAddress),
            ("PsnApiOptions__ProfileAddress", PsnProfileAddress),
            ("PsnApiOptions__ClientId", "test-psn-client"),
            ("PsnApiOptions__ClientAuthorization", "Basic dGVzdA=="),
            ("PsnApiOptions__RedirectUri", PsnRedirectUri),
            ("PsnApiOptions__Scope", "psn:mobile.v2.core psn:clientapp"),
            ("PsnApiOptions__Npsso", "test-npsso"),
        };

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.UseContentRoot(_contentRoot);
            builder.ConfigureLogging(logging => logging.SetMinimumLevel(LogLevel.Warning));
            builder.ConfigureTestServices(services =>
            {
                // every HttpClient of the app, Refit ones included, talks to the fake instead of the network
                services.ConfigureAll<HttpClientFactoryOptions>(options =>
                    options.HttpMessageHandlerBuilderActions.Add(handlerBuilder => handlerBuilder.PrimaryHandler = ExternalApis));
                // the keys of the Steam sign-in state stay in memory instead of the profile of the user running the tests
                services.AddSingleton<IDataProtectionProvider>(new EphemeralDataProtectionProvider());
                services.AddSingleton<IStartupFilter>(new ServerErrorFilter(UnhandledExceptions));
            });
        }

        // the exceptions the app didn't handle; Kestrel answers them with 500, as the filter below does
        public List<Exception> UnhandledExceptions { get; } = new();

        // TestServer passes an unhandled exception to the test instead of answering, the client of the site sees 500
        private class ServerErrorFilter : IStartupFilter
        {
            private readonly List<Exception> _exceptions;

            public ServerErrorFilter(List<Exception> exceptions)
            {
                _exceptions = exceptions;
            }

            public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next) => app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    try
                    {
                        await nextMiddleware();
                    }
                    catch (Exception e) when (!context.Response.HasStarted)
                    {
                        lock (_exceptions)
                        {
                            _exceptions.Add(e);
                        }

                        context.Response.Clear();
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    }
                });
                next(app);
            };
        }

        // every test starts from the database right after the migrations, with the rows the migrations seed
        public async Task ResetAsync()
        {
            ExternalApis.Reset();
            lock (UnhandledExceptions)
            {
                UnhandledExceptions.Clear();
            }
            (Services.GetRequiredService<IMemoryCache>() as MemoryCache)?.Compact(1.0);
            ForgetPsnSignIn();
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            var sql = "SET FOREIGN_KEY_CHECKS = 0;"
                + string.Concat(_tables.Select(table => $"DELETE FROM `{table}`;"))
                + string.Concat(_seededTables.Select(table => $"INSERT INTO `{table}` SELECT * FROM `__seed_{table}`;"))
                + "SET FOREIGN_KEY_CHECKS = 1;";
            await using var command = new MySqlCommand(sql, connection);
            await command.ExecuteNonQueryAsync();
        }

        // the tokens of the service account of PSN and the state of its sign-in live in a singleton, every test starts signed out;
        // a second copy of the app for that won't do: FastEndpoints keeps the services of the app in a static, closing a copy breaks the other
        private void ForgetPsnSignIn()
        {
            var store = Services.GetRequiredService<PsnTokenStore>();
            foreach (var field in typeof(PsnTokenStore).GetFields(BindingFlags.Instance | BindingFlags.NonPublic).Where(f => !f.IsInitOnly))
            {
                field.SetValue(store, field.FieldType.IsValueType ? Activator.CreateInstance(field.FieldType) : null);
            }
        }

        // a separate context for arranging and checking the data, it doesn't share tracked entities with the app
        public async Task<T> WithDbAsync<T>(Func<IgroCehContext, Task<T>> action)
        {
            using var scope = Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IgroCehContext>();
            return await action(db);
        }

        public Task WithDbAsync(Func<IgroCehContext, Task> action) => WithDbAsync(async db =>
        {
            await action(db);
            return true;
        });

        public async Task<long> ScalarAsync(string sql)
        {
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            await using var command = new MySqlCommand(sql, connection);
            return Convert.ToInt64(await command.ExecuteScalarAsync());
        }

        private async Task RememberSeedAsync()
        {
            await using var connection = new MySqlConnection(ConnectionString);
            await connection.OpenAsync();
            var tables = new List<string>();
            await using (var command = new MySqlCommand(
                "SELECT TABLE_NAME FROM information_schema.TABLES WHERE TABLE_SCHEMA = DATABASE() AND TABLE_TYPE = 'BASE TABLE' AND TABLE_NAME <> '__EFMigrationsHistory'",
                connection))
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    tables.Add(reader.GetString(0));
                }
            }

            _tables = tables;
            _seededTables = new List<string>();
            foreach (var table in tables)
            {
                await using var count = new MySqlCommand($"SELECT COUNT(*) FROM `{table}`", connection);
                if (Convert.ToInt64(await count.ExecuteScalarAsync()) > 0)
                {
                    await using var copy = new MySqlCommand($"CREATE TABLE `__seed_{table}` AS SELECT * FROM `{table}`", connection);
                    await copy.ExecuteNonQueryAsync();
                    _seededTables.Add(table);
                }
            }
        }
    }

    [CollectionDefinition(Name, DisableParallelization = true)]
    public class ApiCollection : ICollectionFixture<ApiFactory>
    {
        public const string Name = "API";
    }
}
