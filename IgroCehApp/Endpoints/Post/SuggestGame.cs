using API.Helpers;
using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Post
{
    public class SuggestGame: Endpoint<SuggestGameRequest, Results<Ok<EventRecordObject>, BadRequest<string>>>
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

        public override async Task<Results<Ok<EventRecordObject>, BadRequest<string>>> ExecuteAsync(SuggestGameRequest request, CancellationToken ct)
        {
            var updatedEventRecord = await _eventApplicationService.SuggestGameAsync(request.EventRecordId, request.GameId);
            await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.SuggestGame, new {
                Id = updatedEventRecord.Id,
                Game = updatedEventRecord.Game
            });
            return TypedResults.Ok(updatedEventRecord);
        }
    }
}
