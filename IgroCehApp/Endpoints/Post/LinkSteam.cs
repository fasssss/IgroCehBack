using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the front passes the answer of Steam it was sent to, Steam itself confirms it, then the account is linked
    public class LinkSteam : Endpoint<LinkSteamRequest, Results<Ok<SteamProfileStatusResponse>, BadRequest<string>, Conflict<string>, StatusCodeHttpResult>>
    {
        private readonly ISteamLinkApplicationService _steamLinkApplicationService;
        private readonly SteamLinkState _steamLinkState;

        public LinkSteam(ISteamLinkApplicationService steamLinkApplicationService, SteamLinkState steamLinkState)
        {
            _steamLinkApplicationService = steamLinkApplicationService;
            _steamLinkState = steamLinkState;
        }

        public override void Configure()
        {
            Post("/api/linkSteam");
        }

        public override async Task<Results<Ok<SteamProfileStatusResponse>, BadRequest<string>, Conflict<string>, StatusCodeHttpResult>> ExecuteAsync(
            LinkSteamRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var returnTo = _steamLinkState.GetReturnTo(stringId,
                request.Parameters.GetValueOrDefault("returnPath"), request.Parameters.GetValueOrDefault("state"));
            if (returnTo == null)
            {
                return TypedResults.BadRequest("The sign-in is outdated or was started by another user");
            }

            try
            {
                var (result, isGameDataPublic) = await _steamLinkApplicationService.LinkAsync(stringId, request.Parameters, returnTo, ct);
                switch (result)
                {
                    case LinkSteamResult.InvalidAnswer:
                        return TypedResults.BadRequest("Steam doesn't confirm the sign-in");
                    case LinkSteamResult.TakenByAnotherUser:
                        return TypedResults.Conflict("The Steam account is linked to another user");
                }

                return TypedResults.Ok(new SteamProfileStatusResponse { IsGameDataPublic = isGameDataPublic });
            }
            catch (SteamUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
