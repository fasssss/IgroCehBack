using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Enum;
using Domain.Enums;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the participant of the record guesses its game while the event is at the guessing stage
    public class SuggestGame: Endpoint<SuggestGameRequest, Results<Ok<EventRecordObject>, NotFound, StatusCodeHttpResult, BadRequest<string>>>
    {
        private readonly IEventApplicationService _eventApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public SuggestGame(IEventApplicationService eventApplicationService, WebSocketHelper webSocketHelper)
        {
            _eventApplicationService = eventApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/suggestGame");
        }

        public override async Task<Results<Ok<EventRecordObject>, NotFound, StatusCodeHttpResult, BadRequest<string>>> ExecuteAsync(SuggestGameRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, updatedEventRecord, eventStarted) = await _eventApplicationService.SuggestGameAsync(stringId, request.EventRecordId, request.GameId);
            switch (result)
            {
                case SuggestGameResult.NotFound:
                    return TypedResults.NotFound();
                case SuggestGameResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
                case SuggestGameResult.Unavailable:
                    return TypedResults.BadRequest("The game can't be guessed now");
            }

            await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.SuggestGame, new {
                Id = updatedEventRecord.Id,
                Game = updatedEventRecord.Game
            });
            if (eventStarted)
            {
                await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.UpdateEventStage,
                    new MoveEventToNextStageResponse { MoveToStage = (int)EventStatusId.Active });
            }

            return TypedResults.Ok(updatedEventRecord);
        }
    }
}
