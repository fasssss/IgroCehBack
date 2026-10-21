using API.Helpers;
using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // hides an event with its records (soft delete), available to admins of the event's guild
    public class DeleteEvent : Endpoint<DeleteEventRequest, Results<Ok, NotFound, StatusCodeHttpResult>>
    {
        private readonly IEventApplicationService _eventApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public DeleteEvent(IEventApplicationService eventApplicationService, WebSocketHelper webSocketHelper)
        {
            _eventApplicationService = eventApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/deleteEvent");
        }

        public override async Task<Results<Ok, NotFound, StatusCodeHttpResult>> ExecuteAsync(DeleteEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _eventApplicationService.DeleteEventAsync(stringId, request.EventId);
            switch (result)
            {
                case DeleteEventResult.NotFound:
                    return TypedResults.NotFound();
                case DeleteEventResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
            }

            // everyone who has the event opened leaves it
            await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.DeleteEvent, new { request.EventId });
            return TypedResults.Ok();
        }
    }
}
