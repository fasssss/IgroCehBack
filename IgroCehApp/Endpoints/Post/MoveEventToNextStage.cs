using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class MoveEventToNextStage: Endpoint<MoveEventToNextStageRequest, Results<Ok<MoveEventToNextStageResponse>, BadRequest<string>>>
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

        public override async Task<Results<Ok<MoveEventToNextStageResponse>, BadRequest<string>>> ExecuteAsync(MoveEventToNextStageRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var newEventStage = await _eventApplicationService.MoveEventToNextStageAsync(stringId, request.EventId);
            var response = new MoveEventToNextStageResponse { MoveToStage = newEventStage };
            await _webSocketHelper.SendToRoomAsync($"event{request.EventId}updateEventStage", response);
            return TypedResults.Ok(response);
        }
    }
}
