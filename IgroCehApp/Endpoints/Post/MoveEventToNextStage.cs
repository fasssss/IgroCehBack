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
    // the creator moves the event to the next stage by hand: from the registration to the shuffle and from the shuffle to guessing
    public class MoveEventToNextStage: Endpoint<MoveEventToNextStageRequest, Results<Ok<MoveEventToNextStageResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>>>
    {
        private readonly IEventApplicationService _eventApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public MoveEventToNextStage(IEventApplicationService eventApplicationService, WebSocketHelper webSocketHelper)
        {
            _eventApplicationService = eventApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/moveEventToNextStage");
        }

        public override async Task<Results<Ok<MoveEventToNextStageResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>>> ExecuteAsync(MoveEventToNextStageRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _eventApplicationService.MoveEventToNextStageAsync(stringId, request.EventId, (EventStatusId)request.StatusId);
            switch (result)
            {
                case MoveEventToNextStageResult.NotFound:
                    return TypedResults.NotFound();
                case MoveEventToNextStageResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
                case MoveEventToNextStageResult.Unavailable:
                    return TypedResults.BadRequest("The event can't move to this stage now");
            }

            var response = new MoveEventToNextStageResponse { MoveToStage = request.StatusId };
            await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.UpdateEventStage, response);
            return TypedResults.Ok(response);
        }
    }
}
