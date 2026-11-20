using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the catalog of the aspects of games a player picks what they like from
    public class GetGameAspects : EndpointWithoutRequest<Ok<GetGameAspectsResponse>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public GetGameAspects(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getGameAspects");
        }

        public override async Task<Ok<GetGameAspectsResponse>> ExecuteAsync(CancellationToken ct)
        {
            return TypedResults.Ok(new GetGameAspectsResponse { Aspects = await _playerApplicationService.GetGameAspectsAsync() });
        }
    }
}
