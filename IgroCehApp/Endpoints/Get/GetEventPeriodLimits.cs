using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the bounds of the own dates of a new event as days, so the front doesn't count them; postNewEvent checks the same bounds
    public class GetEventPeriodLimits : Endpoint<GetEventPeriodLimitsRequest, Ok<GetEventPeriodLimitsResponse>>
    {
        private readonly IEventApplicationService _eventApplicationService;

        public GetEventPeriodLimits(IEventApplicationService eventApplicationService)
        {
            _eventApplicationService = eventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getEventPeriodLimits");
        }

        public override Task<Ok<GetEventPeriodLimitsResponse>> ExecuteAsync(GetEventPeriodLimitsRequest request, CancellationToken ct)
        {
            return Task.FromResult(TypedResults.Ok(new GetEventPeriodLimitsResponse
            {
                PeriodLimits = _eventApplicationService.GetEventPeriodLimits(request.StartDate),
            }));
        }
    }
}
