using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Domain.Enums;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class SummarizeEvent : Endpoint<SummarizeEventRequest, Results<Ok<SummarizeEventResponse>, BadRequest<string>>>
    {
        private readonly IEventApplicationService _eventApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public SummarizeEvent(IEventApplicationService eventApplicationService, WebSocketHelper webSocketHelper)
        {
            _eventApplicationService = eventApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/summarizeEvent");
        }

        public override async Task<Results<Ok<SummarizeEventResponse>, BadRequest<string>>> ExecuteAsync(SummarizeEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            await _eventApplicationService.SummarizeEventAsync(stringId, request.EventId);

            // summarizing is silently skipped when user has no rights, so the actual status is checked before notifying the room
            var eventObject = await _eventApplicationService.GetEventByIdAsync(stringId, request.EventId);
            if (eventObject?.StatusId == EventStatusId.Finished)
            {
                await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.UpdateEventStage,
                    new MoveEventToNextStageResponse { MoveToStage = (int)EventStatusId.Finished });
            }

            return TypedResults.Ok(new SummarizeEventResponse { });
        }
    }
}
