using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // the ladder tab of the profile: the Igrofest game the player plays now and the ones they passed;
    // open to the same players as the profile, for others the player doesn't exist
    public class GetPlayerLadder : Endpoint<GetPlayerLadderRequest, Results<Ok<GetPlayerLadderResponse>, NotFound>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public GetPlayerLadder(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getPlayerLadder");
        }

        public override async Task<Results<Ok<GetPlayerLadderResponse>, NotFound>> ExecuteAsync(GetPlayerLadderRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var ladder = await _playerApplicationService.GetPlayerLadderAsync(stringId, request.UserId);
            if (ladder == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new GetPlayerLadderResponse { Fest = ladder.Fest });
        }
    }
}
