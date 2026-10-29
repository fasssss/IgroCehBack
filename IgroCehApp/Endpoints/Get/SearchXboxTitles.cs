using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the games in Xbox with the name of a catalog game, the administrator picks the one its Xbox achievements come from
    public class SearchXboxTitles : Endpoint<SearchXboxTitlesRequest, Results<Ok<SearchXboxTitlesResponse>, StatusCodeHttpResult>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public SearchXboxTitles(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/searchXboxTitles");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<SearchXboxTitlesResponse>, StatusCodeHttpResult>> ExecuteAsync(SearchXboxTitlesRequest request, CancellationToken ct)
        {
            try
            {
                var titles = await _globalEventApplicationService.SearchXboxTitlesAsync(request.Query, ct);
                return TypedResults.Ok(new SearchXboxTitlesResponse { Titles = titles });
            }
            catch (XboxUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
