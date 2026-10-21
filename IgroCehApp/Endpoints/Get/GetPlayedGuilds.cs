using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // guilds of the current user's events for quick access from the guilds page, the latest played first
    public class GetPlayedGuilds : EndpointWithoutRequest<Ok<GetPlayedGuildsResponse>>
    {
        private readonly IGuildApplicationService _guildApplicationService;

        public GetPlayedGuilds(IGuildApplicationService guildApplicationService)
        {
            _guildApplicationService = guildApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getPlayedGuilds");
        }

        public override async Task<Ok<GetPlayedGuildsResponse>> ExecuteAsync(CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var guilds = await _guildApplicationService.GetPlayedGuildsAsync(stringId);

            return TypedResults.Ok(new GetPlayedGuildsResponse { Guilds = guilds });
        }
    }
}
