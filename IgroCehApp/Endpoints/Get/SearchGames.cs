using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // searches games in VideoGamesCritic catalog
    public class SearchGames : Endpoint<SearchGamesRequest, Results<Ok<SearchGamesResponse>, StatusCodeHttpResult>>
    {
        private readonly IGameApplicationService _gameApplicationService;

        public SearchGames(IGameApplicationService gameApplicationService)
        {
            _gameApplicationService = gameApplicationService;
        }

        public override void Configure()
        {
            Get("/api/searchGames");
        }

        public override async Task<Results<Ok<SearchGamesResponse>, StatusCodeHttpResult>> ExecuteAsync(SearchGamesRequest request, CancellationToken ct)
        {
            try
            {
                var games = await _gameApplicationService.SearchCatalogGamesAsync(request.Query, ct);
                return TypedResults.Ok(new SearchGamesResponse { Games = games });
            }
            catch (GameCatalogUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
