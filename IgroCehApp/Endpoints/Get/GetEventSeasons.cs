using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the current and the next season for the creation of an event, counted by the clock of the server
    public class GetEventSeasons : EndpointWithoutRequest<Ok<GetEventSeasonsResponse>>
    {
        private readonly IEventApplicationService _eventApplicationService;

        public GetEventSeasons(IEventApplicationService eventApplicationService)
        {
            _eventApplicationService = eventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getEventSeasons");
        }

        public override Task<Ok<GetEventSeasonsResponse>> ExecuteAsync(CancellationToken ct)
        {
            return Task.FromResult(TypedResults.Ok(new GetEventSeasonsResponse { Seasons = _eventApplicationService.GetEventSeasons() }));
        }
    }
}
