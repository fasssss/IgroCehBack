using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using Domain.Enums;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the creator moves the event from guessing to active once every participant has guessed a game
    public class ActivateEvent : Endpoint<ActivateEventRequest, Results<Ok, NotFound, StatusCodeHttpResult, BadRequest<string>>>
    {
        private readonly IEventApplicationService _eventApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public ActivateEvent(IEventApplicationService eventApplicationService, WebSocketHelper webSocketHelper)
        {
            _eventApplicationService = eventApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/activateEvent");
        }

        public override async Task<Results<Ok, NotFound, StatusCodeHttpResult, BadRequest<string>>> ExecuteAsync(ActivateEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _eventApplicationService.ActivateEventAsync(stringId, request.EventId);
            switch (result)
            {
                case ActivateEventResult.NotFound:
                    return TypedResults.NotFound();
                case ActivateEventResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
                case ActivateEventResult.Unavailable:
                    return TypedResults.BadRequest("The event can be activated only when every participant has guessed a game");
            }

            await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.UpdateEventStage,
                new MoveEventToNextStageResponse { MoveToStage = (int)EventStatusId.Active });

            return TypedResults.Ok();
        }
    }
}
