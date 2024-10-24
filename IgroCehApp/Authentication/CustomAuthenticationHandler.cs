using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace API.Authentication
{
    public class CustomAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public CustomAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder): base(options, logger, encoder)
        {
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var endpoint = Context.GetEndpoint();
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() != null)
            {
                return AuthenticateResult.NoResult();
            }

            if(Request.Cookies.TryGetValue("access_token", out string? accessToken))
            {
                return AuthenticatedUser();
            }

            return AuthenticateResult.Fail("User was not authorized");
        }

        private AuthenticateResult AuthenticatedUser()
        {
            var claims = new[] { new Claim(ClaimTypes.Hash, Request.Cookies["Id"] ?? "") };
            var identity = new ClaimsIdentity(claims, "customAuthenticationScheme");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "customAuthenticationScheme");

            return AuthenticateResult.Success(ticket);
        }
    }
}
