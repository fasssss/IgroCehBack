using API.RepR.Response;
using FastEndpoints;
using Infrastructure.Configurations;
using Microsoft.Extensions.Options;

namespace API.Endpoints.Get
{
    public class GetDiscordAuthLink: EndpointWithoutRequest<GetDiscordAuthLinkResponse>
    {
        private readonly DiscordApiOptions _discordApiOptions;

        public GetDiscordAuthLink(IOptions<DiscordApiOptions> discordApiOptions)
        {
            _discordApiOptions = discordApiOptions.Value;
        }

        public override void Configure()
        {
            Get("/api/getDiscordAuthLink");
            AllowAnonymous();
        }

        public override async Task<GetDiscordAuthLinkResponse> ExecuteAsync(CancellationToken ct)
        {
            var origin = HttpContext.Request.Headers.Origin;
            return new GetDiscordAuthLinkResponse()
            {
                DiscordApiLink = $"{_discordApiOptions.Address}/oauth2/authorize?response_type=code&client_id={_discordApiOptions.ClientId}&scope=identify&redirect_uri={origin}&prompt=consent"
            };
        }
    }
}
