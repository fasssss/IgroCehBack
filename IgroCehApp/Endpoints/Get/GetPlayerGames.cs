using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // the tab "Мои игры" of the profile: the passed or the guessed games of the player, a page at a time;
    // open to the same players as the profile, for others the player doesn't exist
    public class GetPlayerGames : Endpoint<GetPlayerGamesRequest, Results<Ok<GetPlayerGamesResponse>, NotFound>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public GetPlayerGames(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getPlayerGames");
        }

        public override async Task<Results<Ok<GetPlayerGamesResponse>, NotFound>> ExecuteAsync(GetPlayerGamesRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var games = await _playerApplicationService.GetPlayerGamesAsync(stringId, request.UserId, request.Kind.Value,
                request.Sort ?? PlayerGamesSort.Latest, request.StartFrom);
            if (games == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new GetPlayerGamesResponse { Games = games.Games, HasMore = games.HasMore });
        }
    }
}
