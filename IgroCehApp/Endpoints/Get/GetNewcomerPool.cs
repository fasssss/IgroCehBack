using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // the games participants have put into the pool for newcomers, without their authors
    public class GetNewcomerPool : Endpoint<GetNewcomerPoolRequest, Results<Ok<GetNewcomerPoolResponse>, NotFound>>
    {
        private readonly INewcomerPoolApplicationService _newcomerPoolApplicationService;

        public GetNewcomerPool(INewcomerPoolApplicationService newcomerPoolApplicationService)
        {
            _newcomerPoolApplicationService = newcomerPoolApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getNewcomerPool");
        }

        public override async Task<Results<Ok<GetNewcomerPoolResponse>, NotFound>> ExecuteAsync(GetNewcomerPoolRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var pool = await _newcomerPoolApplicationService.GetPoolAsync(stringId, request.EventId);
            if (pool == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new GetNewcomerPoolResponse { Games = pool.Games, MinGames = pool.MinGames, GamesPerPlayer = pool.GamesPerPlayer });
        }
    }
}
