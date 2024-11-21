using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class RemoveFromEvent: Endpoint<RemoveFromEventRequest, Results<Ok<RemoveFromEventResponse>, BadRequest<string>>>
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

        public override async Task<Results<Ok<RemoveFromEventResponse>, BadRequest<string>>> ExecuteAsync(RemoveFromEventRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var eventRecord = await _eventApplicationService.RemoveFromEventAsync(stringId, request.UserId, request.EventId);
            await _webSocketHelper.SendToRoomAsync($"event{request.EventId}", eventRecord, "removeUserFromEvent");
            return TypedResults.Ok(new RemoveFromEventResponse { EventRecordObject = eventRecord });
        }
    }
}
