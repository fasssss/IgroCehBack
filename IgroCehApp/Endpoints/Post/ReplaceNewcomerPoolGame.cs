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
    // the author changes their game of the pool during the registration, unless it has dropped to a newcomer
    public class ReplaceNewcomerPoolGame : Endpoint<ReplaceNewcomerPoolGameRequest, Results<Ok<ChangeNewcomerPoolResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>>>
    {
        private readonly INewcomerPoolApplicationService _newcomerPoolApplicationService;
        private readonly WebSocketHelper _webSocketHelper;

        public ReplaceNewcomerPoolGame(INewcomerPoolApplicationService newcomerPoolApplicationService, WebSocketHelper webSocketHelper)
        {
            _newcomerPoolApplicationService = newcomerPoolApplicationService;
            _webSocketHelper = webSocketHelper;
        }

        public override void Configure()
        {
            Post("/api/replaceNewcomerPoolGame");
        }

        public override async Task<Results<Ok<ChangeNewcomerPoolResponse>, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>>> ExecuteAsync(ReplaceNewcomerPoolGameRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, poolGame) = await _newcomerPoolApplicationService.ReplaceGameAsync(stringId, request.EventId, request.PoolGameId, request.GameId);
            if (result != ChangeNewcomerPoolResult.Changed)
            {
                return NewcomerPoolAnswers.Refusal<Ok<ChangeNewcomerPoolResponse>>(result);
            }

            await NewcomerPoolAnswers.NotifyAsync(_webSocketHelper, request.EventId);
            return TypedResults.Ok(new ChangeNewcomerPoolResponse { PoolGame = poolGame });
        }
    }
}
