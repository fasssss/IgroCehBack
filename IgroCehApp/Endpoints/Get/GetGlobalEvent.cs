using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // a global event with its games and achievements, a finished one too
    public class GetGlobalEvent : Endpoint<GlobalEventRequest, Results<Ok<GlobalEventResponse>, NotFound>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public GetGlobalEvent(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getGlobalEvent");
        }

        public override async Task<Results<Ok<GlobalEventResponse>, NotFound>> ExecuteAsync(GlobalEventRequest request, CancellationToken ct)
        {
            var globalEvent = await _globalEventApplicationService.GetAsync(request.GlobalEventId);
            if (globalEvent == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new GlobalEventResponse { GlobalEvent = globalEvent });
        }
    }
}
