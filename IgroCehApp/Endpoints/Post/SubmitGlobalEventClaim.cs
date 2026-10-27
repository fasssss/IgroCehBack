using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the current user claims the picked game of a global event is passed and gives the profile that shows it
    public class SubmitGlobalEventClaim : Endpoint<SubmitGlobalEventClaimRequest,
        Results<Ok<GlobalEventParticipationResponse>, NotFound, BadRequest<string>, Conflict<string>, UnprocessableEntity<string>>>
    {
        private readonly IGlobalEventParticipationApplicationService _participationApplicationService;

        public SubmitGlobalEventClaim(IGlobalEventParticipationApplicationService participationApplicationService)
        {
            _participationApplicationService = participationApplicationService;
        }

        public override void Configure()
        {
            Post("/api/submitGlobalEventClaim");
        }

        public override async Task<Results<Ok<GlobalEventParticipationResponse>, NotFound, BadRequest<string>, Conflict<string>, UnprocessableEntity<string>>> ExecuteAsync(
            SubmitGlobalEventClaimRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, participation) = await _participationApplicationService.SubmitClaimAsync(stringId, request.GlobalEventId, request.Platform.Value, request.Profile);
            return GlobalEventParticipationResults.ToResult(result, participation);
        }
    }
}
