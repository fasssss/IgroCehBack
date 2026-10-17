using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // saves a game picked from VideoGamesCritic catalog (or finds already saved one), then it is suggested like any other game
    public class CreateGameFromVgc : Endpoint<CreateGameFromVgcRequest, Results<Ok<CreateGameResponse>, NotFound, StatusCodeHttpResult>>
    {
        private readonly IGameApplicationService _gameApplicationService;

        public CreateGameFromVgc(IGameApplicationService gameApplicationService)
        {
            _gameApplicationService = gameApplicationService;
        }

        public override void Configure()
        {
            Post("/api/createGameFromVgc");
        }

        public override async Task<Results<Ok<CreateGameResponse>, NotFound, StatusCodeHttpResult>> ExecuteAsync(CreateGameFromVgcRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                var gameId = await _gameApplicationService.CreateGameFromCatalogAsync(stringId, request.VgcId, ct);
                if (gameId == null)
                {
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok(new CreateGameResponse { GameId = gameId });
            }
            catch (GameCatalogUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
