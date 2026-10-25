using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // the games of the pool that may drop to the current user, if they join the active event as a newcomer
    public class GetNewcomerRoll : Endpoint<GetNewcomerRollRequest, Results<Ok<GetNewcomerRollResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>>>
    {
        private readonly INewcomerPoolApplicationService _newcomerPoolApplicationService;

        public GetNewcomerRoll(INewcomerPoolApplicationService newcomerPoolApplicationService)
        {
            _newcomerPoolApplicationService = newcomerPoolApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getNewcomerRoll");
        }

        public override async Task<Results<Ok<GetNewcomerRollResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>>> ExecuteAsync(GetNewcomerRollRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, roll) = await _newcomerPoolApplicationService.GetRollAsync(stringId, request.EventId);
            if (result != NewcomerJoinResult.Succeeded)
            {
                return NewcomerJoinAnswers.Refusal<Ok<GetNewcomerRollResponse>>(result);
            }

            return TypedResults.Ok(new GetNewcomerRollResponse { Games = roll.Games, HiddenCount = roll.HiddenCount, MinGames = roll.MinGames });
        }
    }
}
