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
    // the current user asks Steam whether the picked game of a global event is passed, a chosen achievement unlocked after the pick passes it
    public class CheckGlobalEventPassing : Endpoint<GlobalEventRequest, Results<Ok<CheckGlobalEventPassingResponse>, NotFound, Conflict<string>, StatusCodeHttpResult>>
    {
        private readonly IGlobalEventParticipationApplicationService _participationApplicationService;

        public CheckGlobalEventPassing(IGlobalEventParticipationApplicationService participationApplicationService)
        {
            _participationApplicationService = participationApplicationService;
        }

        public override void Configure()
        {
            Post("/api/checkGlobalEventPassing");
        }

        public override async Task<Results<Ok<CheckGlobalEventPassingResponse>, NotFound, Conflict<string>, StatusCodeHttpResult>> ExecuteAsync(
            GlobalEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                var (result, check, participation) = await _participationApplicationService.CheckPassingAsync(stringId, request.GlobalEventId, ct);
                return result switch
                {
                    ParticipateGlobalEventResult.Succeeded => TypedResults.Ok(new CheckGlobalEventPassingResponse { PassingCheck = check, Participation = participation }),
                    ParticipateGlobalEventResult.NotFound => TypedResults.NotFound(),
                    _ => TypedResults.Conflict(GlobalEventParticipationResults.ConflictText(result)),
                };
            }
            catch (SteamUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
