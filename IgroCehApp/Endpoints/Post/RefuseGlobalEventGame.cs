using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the current user refuses the picked game of a global event a week after the pick and rolls three new games at once
    public class RefuseGlobalEventGame : Endpoint<GlobalEventRequest,
        Results<Ok<GlobalEventParticipationResponse>, NotFound, BadRequest<string>, Conflict<string>, UnprocessableEntity<string>>>
    {
        private readonly IGlobalEventParticipationApplicationService _participationApplicationService;

        public RefuseGlobalEventGame(IGlobalEventParticipationApplicationService participationApplicationService)
        {
            _participationApplicationService = participationApplicationService;
        }

        public override void Configure()
        {
            Post("/api/refuseGlobalEventGame");
        }

        public override async Task<Results<Ok<GlobalEventParticipationResponse>, NotFound, BadRequest<string>, Conflict<string>, UnprocessableEntity<string>>> ExecuteAsync(
            GlobalEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, participation) = await _participationApplicationService.RefuseAsync(stringId, request.GlobalEventId, ct);
            return GlobalEventParticipationResults.ToResult(result, participation);
        }
    }
}
