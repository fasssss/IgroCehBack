using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // whether Steam gives the achievements of the linked account, the user can open the game details and check again
    public class GetSteamProfileStatus : EndpointWithoutRequest<Results<Ok<SteamProfileStatusResponse>, NotFound, StatusCodeHttpResult>>
    {
        private readonly ISteamLinkApplicationService _steamLinkApplicationService;

        public GetSteamProfileStatus(ISteamLinkApplicationService steamLinkApplicationService)
        {
            _steamLinkApplicationService = steamLinkApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getSteamProfileStatus");
        }

        public override async Task<Results<Ok<SteamProfileStatusResponse>, NotFound, StatusCodeHttpResult>> ExecuteAsync(CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                var isGameDataPublic = await _steamLinkApplicationService.IsGameDataPublicAsync(stringId, ct);
                return isGameDataPublic == null
                    ? TypedResults.NotFound()
                    : TypedResults.Ok(new SteamProfileStatusResponse { IsGameDataPublic = isGameDataPublic });
            }
            catch (SteamUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
