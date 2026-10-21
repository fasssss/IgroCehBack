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
    // the one who guessed the game marks it as passed by the player it was guessed for
    public class SubmitPassing: Endpoint<SubmitPassingRequest, Results<Ok<SubmitPassingResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>>>
    {
        private readonly IEventApplicationService _eventApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public SubmitPassing(IEventApplicationService eventApplicationService, WebSocketHelper webSocketHelper)
        {
            _eventApplicationService = eventApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/submitPassing");
        }

        public override async Task<Results<Ok<SubmitPassingResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>>> ExecuteAsync(SubmitPassingRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, eventRecordObject) = await _eventApplicationService.SubmitPassingAsync(stringId, request.EventRecordId);
            switch (result)
            {
                case SubmitPassingResult.NotFound:
                    return TypedResults.NotFound();
                case SubmitPassingResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
                case SubmitPassingResult.Unavailable:
                    return TypedResults.BadRequest("The game can't be marked as passed now");
            }

            await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.SubmitPassing, new {
                Id = eventRecordObject.Id,
                Reward = eventRecordObject.Reward
            });

            return TypedResults.Ok(new SubmitPassingResponse { EventRecordObject = eventRecordObject });
        }
    }
}
