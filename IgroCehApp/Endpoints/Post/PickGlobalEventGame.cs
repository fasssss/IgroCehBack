using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the current user picks one of the games they rolled in a global event
    public class PickGlobalEventGame : Endpoint<PickGlobalEventGameRequest,
        Results<Ok<GlobalEventParticipationResponse>, NotFound, BadRequest<string>, Conflict<string>, UnprocessableEntity<string>>>
    {
        private readonly IGlobalEventParticipationApplicationService _participationApplicationService;

        public PickGlobalEventGame(IGlobalEventParticipationApplicationService participationApplicationService)
        {
            _participationApplicationService = participationApplicationService;
        }

        public override void Configure()
        {
            Post("/api/pickGlobalEventGame");
        }

        public override async Task<Results<Ok<GlobalEventParticipationResponse>, NotFound, BadRequest<string>, Conflict<string>, UnprocessableEntity<string>>> ExecuteAsync(
            PickGlobalEventGameRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, participation) = await _participationApplicationService.PickAsync(stringId, request.GlobalEventId, request.AppId);
            return GlobalEventParticipationResults.ToResult(result, participation);
        }
    }
}
