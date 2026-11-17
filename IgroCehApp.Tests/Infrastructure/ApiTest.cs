using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.IdentityModel.Tokens;
using Persistence.Context;

namespace IgroCehApp.Tests.Infrastructure
{
    // a test of the API: the database is reset before it, the requests go as the signed-in users the test names
    [Collection(ApiCollection.Name)]
    public abstract class ApiTest : IAsyncLifetime
    {
        protected ApiTest(ApiFactory factory)
        {
            Factory = factory;
        }

        protected ApiFactory Factory { get; }

        protected FakeExternalApis External => Factory.ExternalApis;

        public virtual Task InitializeAsync() => Factory.ResetAsync();

        public virtual Task DisposeAsync() => Task.CompletedTask;

        // the requests of a user signed in as AuthorizeByCode signs them in: the cookie with the token of the site
        protected ApiClient As(string userId) => new(CreateClient(), Token(userId), Factory.UnhandledExceptions);

        protected ApiClient Anonymous => new(CreateClient(), null, Factory.UnhandledExceptions);

        protected Task Db(Func<IgroCehContext, Task> action) => Factory.WithDbAsync(action);

        protected Task<T> Db<T>(Func<IgroCehContext, Task<T>> action) => Factory.WithDbAsync(action);

        // the rows are added and saved at once
        protected Task Seed(params object[] entities) => Factory.WithDbAsync(async db =>
        {
            db.AddRange(entities);
            await db.SaveChangesAsync();
        });

        public static string Token(string userId, DateTime? expires = null, string secret = ApiFactory.JwtSecret)
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.CreateToken(new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId),
                    new Claim("OAuthAccessToken", "discord-access"),
                    new Claim("OAuthRefreshToken", "discord-refresh"),
                }),
                NotBefore = expires?.AddHours(-2),
                Expires = expires ?? DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature),
                Issuer = ApiFactory.JwtIssuer,
                Audience = ApiFactory.JwtAudience,
            });
            return handler.WriteToken(token);
        }

        private HttpClient CreateClient() => Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost"),
            HandleCookies = false,
            AllowAutoRedirect = false,
        });
    }

    public class ApiClient
    {
        private readonly string? _token;
        private readonly List<Exception> _exceptions;

        public ApiClient(HttpClient http, string? token, List<Exception> exceptions)
        {
            Http = http;
            _token = token;
            _exceptions = exceptions;
        }

        public HttpClient Http { get; }

        public Task<ApiResult> Get(string path) => Send(new HttpRequestMessage(HttpMethod.Get, path));

        public Task<ApiResult> Post(string path, object? body = null) => Send(new HttpRequestMessage(HttpMethod.Post, path)
        {
            Content = JsonContent.Create(body ?? new { }),
        });

        public Task<ApiResult> PostForm(string path, MultipartFormDataContent form) => Send(new HttpRequestMessage(HttpMethod.Post, path) { Content = form });

        public async Task<ApiResult> Send(HttpRequestMessage request)
        {
            if (_token != null)
            {
                request.Headers.Add("Cookie", $"application_token={_token}");
            }

            int before;
            lock (_exceptions)
            {
                before = _exceptions.Count;
            }

            using var response = await Http.SendAsync(request);
            Exception? exception;
            lock (_exceptions)
            {
                exception = _exceptions.Skip(before).FirstOrDefault();
            }

            return new ApiResult(response.StatusCode, await response.Content.ReadAsStringAsync(), response.Content.Headers.ContentType?.MediaType,
                response.Headers.Concat(response.Content.Headers).ToDictionary(h => h.Key, h => string.Join(";", h.Value), StringComparer.OrdinalIgnoreCase),
                await response.Content.ReadAsByteArrayAsync(), exception);
        }
    }

    // Exception is the one the app didn't handle while answering, the reason of a 500
    public record ApiResult(HttpStatusCode Status, string Body, string? ContentType, Dictionary<string, string> Headers, byte[] Bytes, Exception? Exception)
    {
        public JsonNode Json => JsonNode.Parse(Body) ?? throw new InvalidOperationException("The answer is empty");

        // the text of a refusal, the API sends it as a JSON string
        public string Text => Body.StartsWith('"') ? Json.GetValue<string>() : Body;

        public ApiResult Is(HttpStatusCode status)
        {
            Assert.True(Status == status, $"Expected {(int)status} {status}, got {(int)Status} {Status}: {Body}{(Exception == null ? "" : Environment.NewLine + Exception)}");
            return this;
        }

        public JsonNode Ok() => Is(HttpStatusCode.OK).Body.Length == 0 ? new JsonObject() : Json;
    }
}
