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
    // the current user asks a platform whether the picked game of a global event is passed, a chosen achievement on that platform
    // unlocked after the pick passes it; 429 when the player checked on the platform less than GlobalEventLimits.PassingCheckDelayMinutes ago
    public class CheckGlobalEventPassing : Endpoint<CheckGlobalEventPassingRequest, Results<Ok<CheckGlobalEventPassingResponse>, NotFound, Conflict<string>, StatusCodeHttpResult>>
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
            CheckGlobalEventPassingRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                var (result, check, participation) = await _participationApplicationService.CheckPassingAsync(stringId, request.GlobalEventId, request.Platform, ct);
                return result switch
                {
                    ParticipateGlobalEventResult.Succeeded => TypedResults.Ok(new CheckGlobalEventPassingResponse { PassingCheck = check, Participation = participation }),
                    ParticipateGlobalEventResult.NotFound => TypedResults.NotFound(),
                    ParticipateGlobalEventResult.CheckedRecently => TypedResults.StatusCode(StatusCodes.Status429TooManyRequests),
                    _ => TypedResults.Conflict(GlobalEventParticipationResults.ConflictText(result, request.Platform)),
                };
            }
            catch (AchievementPlatformUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
