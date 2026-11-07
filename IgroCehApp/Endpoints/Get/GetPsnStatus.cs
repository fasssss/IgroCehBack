using API.Authorization;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // whether the service account of PSN works and when its NPSSO ends, the administrator renews the NPSSO by hand
    public class GetPsnStatus : EndpointWithoutRequest<Ok<GetPsnStatusResponse>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public GetPsnStatus(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getPsnStatus");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Ok<GetPsnStatusResponse>> ExecuteAsync(CancellationToken ct)
        {
            return TypedResults.Ok(new GetPsnStatusResponse { PsnStatus = await _globalEventApplicationService.GetPsnStatusAsync(ct) });
        }
    }
}
