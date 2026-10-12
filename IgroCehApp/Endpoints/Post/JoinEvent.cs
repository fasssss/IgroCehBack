using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class JoinEvent: Endpoint<JoinEventRequest, Results<Ok<JoinEventResponse>, BadRequest>>
    {
        private readonly IEventApplicationService _eventApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public JoinEvent(IEventApplicationService eventApplicationService, WebSocketHelper webSocketHelper)
        {
            _eventApplicationService = eventApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/joinEvent");
        }

        public override async Task<Results<Ok<JoinEventResponse>, BadRequest>> ExecuteAsync(JoinEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var eventRecord = await _eventApplicationService.JoinEventAsync(stringId, request.EventId);
            if (eventRecord == null)
            {
                return TypedResults.BadRequest();
            }

            await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.AddUserToEvent, eventRecord);
            return TypedResults.Ok(new JoinEventResponse { EventRecordObject = eventRecord });
        }
    }
}
