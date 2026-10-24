using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the catalog of genres and release periods a player picks preferences from
    public class GetPreferenceTags : EndpointWithoutRequest<Ok<GetPreferenceTagsResponse>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public GetPreferenceTags(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getPreferenceTags");
        }

        public override async Task<Ok<GetPreferenceTagsResponse>> ExecuteAsync(CancellationToken ct)
        {
            return TypedResults.Ok(new GetPreferenceTagsResponse { Tags = await _playerApplicationService.GetPreferenceTagsAsync() });
        }
    }
}
