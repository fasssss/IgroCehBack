using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class UnlinkSteam : EndpointWithoutRequest<Ok>
    {
        private readonly ISteamLinkApplicationService _steamLinkApplicationService;

        public UnlinkSteam(ISteamLinkApplicationService steamLinkApplicationService)
        {
            _steamLinkApplicationService = steamLinkApplicationService;
        }

        public override void Configure()
        {
            Post("/api/unlinkSteam");
        }

        public override async Task<Ok> ExecuteAsync(CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            await _steamLinkApplicationService.UnlinkAsync(stringId);
            return TypedResults.Ok();
        }
    }
}
