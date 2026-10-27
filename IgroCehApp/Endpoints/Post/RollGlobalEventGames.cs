using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the current user rolls three games of an active global event, once; 422 when every game has their unlocked achievement
    public class RollGlobalEventGames : Endpoint<GlobalEventRequest,
        Results<Ok<GlobalEventParticipationResponse>, NotFound, BadRequest<string>, Conflict<string>, UnprocessableEntity<string>>>
    {
        private readonly IGlobalEventParticipationApplicationService _participationApplicationService;

        public RollGlobalEventGames(IGlobalEventParticipationApplicationService participationApplicationService)
        {
            _participationApplicationService = participationApplicationService;
        }

        public override void Configure()
        {
            Post("/api/rollGlobalEventGames");
        }

        public override async Task<Results<Ok<GlobalEventParticipationResponse>, NotFound, BadRequest<string>, Conflict<string>, UnprocessableEntity<string>>> ExecuteAsync(
            GlobalEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, participation) = await _participationApplicationService.RollAsync(stringId, request.GlobalEventId, ct);
            return GlobalEventParticipationResults.ToResult(result, participation);
        }
    }
}
