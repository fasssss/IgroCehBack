using API.Configurations;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using Domain.Entities;
using FastEndpoints;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace API.Endpoints.Post
{
    public class AuthorizeByCode: Endpoint<AuthorizeByCodeRequest, Results<Ok<UserObject>, BadRequest>>
    {
        private readonly DiscordApiOptions _discordApiOptions;
        private readonly ApplicationJwtOptions _applicationJwtOptions;
        private readonly IAuthorizationApplicationService _authorizationApplicationService;

        public AuthorizeByCode(
            IOptions<DiscordApiOptions> discordApiOptions,
            IOptions<ApplicationJwtOptions> applicationJwtOptions,
            IAuthorizationApplicationService authorizationApplicationService)
        {
            _discordApiOptions = discordApiOptions.Value;
            _applicationJwtOptions = applicationJwtOptions.Value;
            _authorizationApplicationService = authorizationApplicationService;
        }

        public override void Configure()
        {
            Get("/api/authorizeByCode");
            AllowAnonymous();
        }

        public override async Task<Results<Ok<UserObject>, BadRequest>> ExecuteAsync(AuthorizeByCodeRequest authCodeRequest, CancellationToken ct)
        {
            var authCode = authCodeRequest.AuthorizationCode;
            var data = await _authorizationApplicationService.AuthorizeAsync(authCode);

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_applicationJwtOptions.Secret);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, data.UserObject.Id),
                    new Claim("OAuthAccessToken", data.AuthorizationTokens.AccessToken),
                    new Claim("OAuthRefreshToken", data.AuthorizationTokens.RefreshToken)
                }),
                Expires = DateTime.UtcNow.AddHours(int.Parse(_applicationJwtOptions.ExpInHours)),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _applicationJwtOptions.Issuer,
                Audience = _applicationJwtOptions.Audience
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var stringToken = tokenHandler.WriteToken(token);

            HttpContext.Response.Cookies.Append("application_token", stringToken, new CookieOptions
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
