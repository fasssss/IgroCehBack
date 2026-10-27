using API.RepR.Response;
using Application.DTO;
using Application.Enum;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Helpers
{
    // the answers of rolling, picking and refusing the games of a global event: a state the roll is not in is a conflict,
    // the front reads the participation again
    public static class GlobalEventParticipationResults
    {
        public static Results<Ok<GlobalEventParticipationResponse>, NotFound, BadRequest<string>, Conflict<string>, UnprocessableEntity<string>> ToResult(
            ParticipateGlobalEventResult result, GlobalEventParticipationObject participation) => result switch
        {
            ParticipateGlobalEventResult.Succeeded => TypedResults.Ok(new GlobalEventParticipationResponse { Participation = participation }),
            ParticipateGlobalEventResult.NotFound => TypedResults.NotFound(),
            ParticipateGlobalEventResult.NotRolledGame => TypedResults.BadRequest("The game isn't among the rolled ones"),
            ParticipateGlobalEventResult.NoGames => TypedResults.UnprocessableEntity("The player has unlocked an achievement of every game they could get"),
            ParticipateGlobalEventResult.Finished => TypedResults.Conflict("The event is finished"),
            ParticipateGlobalEventResult.AlreadyRolled => TypedResults.Conflict("The games are rolled already"),
            ParticipateGlobalEventResult.NotRolled => TypedResults.Conflict("The games aren't rolled yet"),
            ParticipateGlobalEventResult.AlreadyPicked => TypedResults.Conflict("A game is picked already"),
            ParticipateGlobalEventResult.NotPicked => TypedResults.Conflict("No game is picked"),
            ParticipateGlobalEventResult.TooEarly => TypedResults.Conflict("The picked game can't be refused yet"),
            _ => TypedResults.Conflict("The roll was changed by another request"),
        };
    }
}
