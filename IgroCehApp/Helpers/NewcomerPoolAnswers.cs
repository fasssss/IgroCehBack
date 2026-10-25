using Application.Enum;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Helpers
{
    // the answers the endpoints changing the pool for newcomers share
    public static class NewcomerPoolAnswers
    {
        public static Results<TOk, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>> Refusal<TOk>(ChangeNewcomerPoolResult result) where TOk : IResult
        {
            switch (result)
            {
                case ChangeNewcomerPoolResult.NotFound:
                    return TypedResults.NotFound();
                case ChangeNewcomerPoolResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
                case ChangeNewcomerPoolResult.AlreadyInPool:
                    return TypedResults.Conflict("The game is already in the pool");
                case ChangeNewcomerPoolResult.Dropped:
                    return TypedResults.Conflict("The game has already dropped to a newcomer");
                case ChangeNewcomerPoolResult.LimitReached:
                    return TypedResults.BadRequest("You have put as many games as one may");
                default:
                    return TypedResults.BadRequest("The pool can't be changed now");
            }
        }

        // the viewers of the pool fetch it again
        public static Task NotifyAsync(WebSocketHelper webSocketHelper, string eventId) =>
            webSocketHelper.SendToRoomAsync(WebSocketRooms.Event(eventId), EventRoomMessages.UpdateNewcomerPool, new { EventId = eventId });
    }
}
