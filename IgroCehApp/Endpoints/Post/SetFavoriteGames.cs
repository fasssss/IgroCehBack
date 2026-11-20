using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the current user saves their favorite games
    public class SetFavoriteGames : Endpoint<SetFavoriteGamesRequest, Results<Ok, BadRequest<string>>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public SetFavoriteGames(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Post("/api/setFavoriteGames");
        }

        public override async Task<Results<Ok, BadRequest<string>>> ExecuteAsync(SetFavoriteGamesRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _playerApplicationService.SetFavoriteGamesAsync(stringId, request.GameIds);
            if (result == SetFavoriteGamesResult.UnknownGame)
            {
                return TypedResults.BadRequest("There is no such game");
            }

            return TypedResults.Ok();
        }
    }
}
