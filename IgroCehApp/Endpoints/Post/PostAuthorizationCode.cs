using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace API.Endpoints.Post
{
    public class PostAuthorizationCode: Endpoint<PostAuthorizationCodeRequest, Results<Ok<UserObject>, BadRequest>>
    {
        private readonly DiscordApiOptions _discordApiOptions;
        private readonly IAuthorizationApplicationService _authorizationApplicationService;

        public PostAuthorizationCode(
            IOptions<DiscordApiOptions> discordApiOptions,
            IAuthorizationApplicationService authorizationApplicationService)
        {
            _discordApiOptions = discordApiOptions.Value;
            _authorizationApplicationService = authorizationApplicationService;
        }

        public override void Configure()
        {
            Post("/api/postAuthorizationCode");
            AllowAnonymous();
        }

        public override async Task<Results<Ok<UserObject>, BadRequest>> ExecuteAsync(PostAuthorizationCodeRequest authCodeRequest, CancellationToken ct)
        {
            var authCode = authCodeRequest.AuthorizationCode;
            var data = await _authorizationApplicationService.Authorize(authCode);
            HttpContext.Response.Cookies.Append("access_token", data.AuthorizationTokens.AccessToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddSeconds(data.AuthorizationTokens.ExpiresIn)
            });
            HttpContext.Response.Cookies.Append("refresh_token", data.AuthorizationTokens.RefreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
            });
            HttpContext.Response.Cookies.Append("id", data.UserObject.Id.ToString(), new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = Microsoft.AspNetCore.Http.SameSiteMode.None,
                Expires = DateTimeOffset.UtcNow.AddSeconds(data.AuthorizationTokens.ExpiresIn)
            });
            return TypedResults.Ok(data.UserObject);
        }
    }
}
