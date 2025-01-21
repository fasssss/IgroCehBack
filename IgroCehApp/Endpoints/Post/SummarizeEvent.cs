using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class SummarizeEvent : Endpoint<SummarizeEventRequest, Results<Ok<SummarizeEventResponse>, BadRequest<string>>>
    {
        private readonly IEventApplicationService _eventApplicationService;

        public SummarizeEvent(IEventApplicationService eventApplicationService, IHostEnvironment hostingEnvironment)
        {
            _eventApplicationService = eventApplicationService;
        }

        public override void Configure()
        {
            Post("/api/summarizeEvent");
        }

        public override async Task<Results<Ok<SummarizeEventResponse>, BadRequest<string>>> ExecuteAsync(SummarizeEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var eventRecordObject = await _eventApplicationService.SubmitPassingAsync(stringId, request.EventRecordId);

            return TypedResults.Ok(new SummarizeEventResponse { });
        }
    }
}
