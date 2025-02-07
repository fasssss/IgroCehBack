using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class ShuffleUsers : Endpoint<ShuffleUsersRequest, Results<Ok<ShuffleUsersResponse>, BadRequest>>
    {
        private readonly IEventApplicationService _eventApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public ShuffleUsers(IEventApplicationService eventApplicationService, WebSocketHelper webSocketHelper)
        {
            _eventApplicationService = eventApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/shuffleUsers");
        }

        public override async Task<Results<Ok<ShuffleUsersResponse>, BadRequest>> ExecuteAsync(ShuffleUsersRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var eventRecords = await _eventApplicationService.ShuffleUsersAsync(stringId, request.EventId);
            await _webSocketHelper.SendToRoomAsync($"event{request.EventId}ShuffleUsers", eventRecords, "shuffleUsers");
            return TypedResults.Ok(new ShuffleUsersResponse { EventRecordObjects = eventRecords });
        }
    }
}
