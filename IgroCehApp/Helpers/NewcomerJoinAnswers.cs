using Application.Enum;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Helpers
{
    // the refusals the endpoints of a newcomer share
    public static class NewcomerJoinAnswers
    {
        public static Results<TOk, NotFound, StatusCodeHttpResult, BadRequest<string>, Conflict<string>> Refusal<TOk>(NewcomerJoinResult result) where TOk : IResult
        {
            switch (result)
            {
                case NewcomerJoinResult.NotFound:
                    return TypedResults.NotFound();
                case NewcomerJoinResult.Forbidden:
                    return TypedResults.StatusCode(StatusCodes.Status403Forbidden);
                case NewcomerJoinResult.AlreadyJoined:
                    return TypedResults.Conflict("You are in the event already");
                case NewcomerJoinResult.TooManyExcluded:
                    return TypedResults.BadRequest("Too many games are crossed out");
                default:
                    return TypedResults.BadRequest("Newcomers can't join the event now");
            }
        }
    }
}
