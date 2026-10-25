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
    // a player leaves the event or the creator removes them, only during the registration
    public class RemoveFromEvent: Endpoint<RemoveFromEventRequest, Results<Ok<RemoveFromEventResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>>>
    {
        private readonly IEventApplicationService _eventApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public RemoveFromEvent(IEventApplicationService eventApplicationService, WebSocketHelper webSocketHelper)
        {
            _eventApplicationService = eventApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/removeFromEvent");
        }

        public override async Task<Results<Ok<RemoveFromEventResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>>> ExecuteAsync(RemoveFromEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, eventRecord) = await _eventApplicationService.RemoveFromEventAsync(stringId, request.UserId, request.EventId);
            switch (result)
            {
                case RemoveFromEventResult.NotFound:
                    return TypedResults.NotFound();
                case RemoveFromEventResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
                case RemoveFromEventResult.Unavailable:
                    return TypedResults.BadRequest("Players leave the event only during the registration");
            }

            await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.RemoveUserFromEvent, eventRecord);
            return TypedResults.Ok(new RemoveFromEventResponse { EventRecordObject = eventRecord });
        }
    }
}
