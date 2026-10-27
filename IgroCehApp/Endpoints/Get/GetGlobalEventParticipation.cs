using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // the games the current user rolled in a global event and the one they picked
    public class GetGlobalEventParticipation : Endpoint<GlobalEventRequest, Results<Ok<GlobalEventParticipationResponse>, NotFound>>
    {
        private readonly IGlobalEventParticipationApplicationService _participationApplicationService;

        public GetGlobalEventParticipation(IGlobalEventParticipationApplicationService participationApplicationService)
        {
            _participationApplicationService = participationApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getGlobalEventParticipation");
        }

        public override async Task<Results<Ok<GlobalEventParticipationResponse>, NotFound>> ExecuteAsync(GlobalEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, participation) = await _participationApplicationService.GetAsync(stringId, request.GlobalEventId);
            if (result == ParticipateGlobalEventResult.NotFound)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new GlobalEventParticipationResponse { Participation = participation });
        }
    }
}
