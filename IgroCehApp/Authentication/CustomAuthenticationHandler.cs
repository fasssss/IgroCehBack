using API.Configurations;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;

namespace API.Authentication
{
    public class CustomAuthenticationHandler : AuthenticationHandler<ApplicationJwtOptions>
    {
        private readonly ApplicationJwtOptions _applicationJwtOptions;

        public CustomAuthenticationHandler(
            IOptionsMonitor<ApplicationJwtOptions> applicationJwtOptions,
            ILoggerFactory logger,
            UrlEncoder encoder): base(applicationJwtOptions, logger, encoder)
        {
            _applicationJwtOptions = applicationJwtOptions.CurrentValue;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }

            if (Request.Cookies.TryGetValue("application_token", out string? applicationToken))
            {
                var handler = new JwtSecurityTokenHandler();
                var tokenValidationResult = await handler.ValidateTokenAsync(applicationToken, new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidateIssuer = true,
                    ValidateIssuerSigningKey = true,
                    ValidateTokenReplay = false,
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_applicationJwtOptions.Secret)),
                    ValidIssuer = _applicationJwtOptions.Issuer,
                    ValidAudience = _applicationJwtOptions.Audience,
                });

                if (tokenValidationResult.IsValid)
                {
                    tokenValidationResult.Claims.TryGetValue(ClaimTypes.NameIdentifier, out var userId);
                    tokenValidationResult.Claims.TryGetValue("OAuthAccessToken", out var accessToken);
                    tokenValidationResult.Claims.TryGetValue("OAuthRefreshToken", out var refreshToken);
                    return AuthenticatedUser(userId?.ToString(), accessToken?.ToString(), refreshToken?.ToString());
                }
            }

            return AuthenticateResult.Fail("User was not authorized");
        }

        private AuthenticateResult AuthenticatedUser(string? userId, string? accessToken, string? refreshToken)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId ?? ""),
                new Claim("OAuthAccessToken", accessToken ?? ""),
                new Claim("OAuthRefreshToken", refreshToken ?? ""),
            };
            var identity = new ClaimsIdentity(claims, "customAuthenticationScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "customAuthenticationScheme");

            return AuthenticateResult.Success(ticket);
        }
    }
}
