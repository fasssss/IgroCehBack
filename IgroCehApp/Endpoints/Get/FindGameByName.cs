using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    public class FindGameByName: Endpoint<FindGameByNameRequest, Results<Ok<FindGameByNameResponse>, BadRequest<string>>>
    {
        private readonly IGameApplicationService _gameApplicationService;

        public FindGameByName(IGameApplicationService gameApplicationService)
        {
            _gameApplicationService = gameApplicationService;
        }

        public override void Configure()
        {
            Get("/api/findGameByName");
        }

        public override async Task<Results<Ok<FindGameByNameResponse>, BadRequest<string>>> ExecuteAsync(FindGameByNameRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (stringId != null)
            {
                var existingGame = await _gameApplicationService.FindGameByNameAsync(request.Name);
                return TypedResults.Ok(new FindGameByNameResponse { gameObject = existingGame });
            }

            return TypedResults.Ok(new FindGameByNameResponse());
        }
    }
}
