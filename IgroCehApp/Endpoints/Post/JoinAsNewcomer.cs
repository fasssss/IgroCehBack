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
    // a member of the guild joins the active event and rolls a game of the pool; the record comes to the event room like any joined player
    public class JoinAsNewcomer : Endpoint<JoinAsNewcomerRequest, Results<Ok<JoinAsNewcomerResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>>>
    {
        private readonly INewcomerPoolApplicationService _newcomerPoolApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public JoinAsNewcomer(INewcomerPoolApplicationService newcomerPoolApplicationService, WebSocketHelper webSocketHelper)
        {
            _newcomerPoolApplicationService = newcomerPoolApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/joinAsNewcomer");
        }

        public override async Task<Results<Ok<JoinAsNewcomerResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>>> ExecuteAsync(JoinAsNewcomerRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, eventRecord) = await _newcomerPoolApplicationService.JoinAsync(stringId, request.EventId, request.ExcludedPoolGameIds);
            if (result != NewcomerJoinResult.Succeeded)
            {
                return NewcomerJoinAnswers.Refusal<Ok<JoinAsNewcomerResponse>>(result);
            }

            await _webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(request.EventId), EventRoomMessages.AddUserToEvent, eventRecord);
            return TypedResults.Ok(new JoinAsNewcomerResponse { EventRecordObject = eventRecord });
        }
    }
}
