using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the title id of the game in Xbox the administrator chose among the found ones; 404 when the product of the store has none
    // (a bundle, an add-on, a PC listing)
    public class GetXboxTitleId : Endpoint<GetXboxTitleIdRequest, Results<Ok<GetXboxTitleIdResponse>, NotFound, StatusCodeHttpResult>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public GetXboxTitleId(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getXboxTitleId");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<GetXboxTitleIdResponse>, NotFound, StatusCodeHttpResult>> ExecuteAsync(GetXboxTitleIdRequest request,
            CancellationToken ct)
        {
            try
            {
                var titleId = await _globalEventApplicationService.GetXboxTitleIdAsync(request.ProductId, ct);
                return titleId == null ? TypedResults.NotFound() : TypedResults.Ok(new GetXboxTitleIdResponse { TitleId = titleId });
            }
            catch (XboxUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
