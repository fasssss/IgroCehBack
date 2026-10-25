using API.Helpers;
using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the author takes their game out of the pool during the registration, unless it has dropped to a newcomer
    public class RemoveNewcomerPoolGame : Endpoint<RemoveNewcomerPoolGameRequest, Results<Ok, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>>>
    {
        private readonly INewcomerPoolApplicationService _newcomerPoolApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public RemoveNewcomerPoolGame(INewcomerPoolApplicationService newcomerPoolApplicationService, WebSocketHelper webSocketHelper)
        {
            _newcomerPoolApplicationService = newcomerPoolApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/removeNewcomerPoolGame");
        }

        public override async Task<Results<Ok, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>>> ExecuteAsync(RemoveNewcomerPoolGameRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _newcomerPoolApplicationService.RemoveGameAsync(stringId, request.EventId, request.PoolGameId);
            if (result != ChangeNewcomerPoolResult.Changed)
            {
                return NewcomerPoolAnswers.Refusal<Ok>(result);
            }

            await NewcomerPoolAnswers.NotifyAsync(_webSocketHelper, request.EventId);
            return TypedResults.Ok();
        }
    }
}
