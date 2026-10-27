using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the global event everyone sees on the main page, the answer is empty when none is active
    public class GetActiveGlobalEvent : EndpointWithoutRequest<Ok<GlobalEventResponse>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public GetActiveGlobalEvent(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getActiveGlobalEvent");
        }

        public override async Task<Ok<GlobalEventResponse>> ExecuteAsync(CancellationToken ct)
        {
            return TypedResults.Ok(new GlobalEventResponse { GlobalEvent = await _globalEventApplicationService.GetActiveAsync() });
        }
    }
}
