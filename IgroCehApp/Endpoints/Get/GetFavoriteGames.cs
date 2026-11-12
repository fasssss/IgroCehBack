using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // "Favorite games" of the profile; open to the same players as the profile, for others the player doesn't exist
    public class GetFavoriteGames : Endpoint<GetFavoriteGamesRequest, Results<Ok<GetFavoriteGamesResponse>, NotFound>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public GetFavoriteGames(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getFavoriteGames");
        }

        public override async Task<Results<Ok<GetFavoriteGamesResponse>, NotFound>> ExecuteAsync(GetFavoriteGamesRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var games = await _playerApplicationService.GetFavoriteGamesAsync(stringId, request.UserId);
            if (games == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new GetFavoriteGamesResponse { Games = games });
        }
    }
}
