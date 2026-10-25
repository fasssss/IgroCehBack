using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // a member of the guild joins the event while it gathers players
    public class JoinEvent: Endpoint<JoinEventRequest, Results<Ok<JoinEventResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>>>
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

        public override async Task<Results<Ok<JoinEventResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>>> ExecuteAsync(JoinEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, eventRecord) = await _eventApplicationService.JoinEventAsync(stringId, request.EventId);
            switch (result)
            {
                case JoinEventResult.NotFound:
                    return TypedResults.NotFound();
                case JoinEventResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
                case JoinEventResult.Unavailable:
                    return TypedResults.BadRequest("The event doesn't gather players any more");
                case JoinEventResult.AlreadyJoined:
                    return TypedResults.Conflict("You have already joined the event");
            }

            await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.AddUserToEvent, eventRecord);
            return TypedResults.Ok(new JoinEventResponse { EventRecordObject = eventRecord });
        }
    }
}
