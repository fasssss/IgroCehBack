using API.RepR.Response;
using Application.DTO;
using Application.Enum;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Helpers
{
    // the answers of rolling, picking, refusing and proving the passing of the games of a global event: a state the participation is not in
    // is a conflict, the front reads the participation again
    public static class GlobalEventParticipationResults
    {
        public static Results<Ok<GlobalEventParticipationResponse>, NotFound, BadRequest<string>, Conflict<string>, UnprocessableEntity<string>> ToResult(
            ParticipateGlobalEventResult result, GlobalEventParticipationObject participation) => result switch
        {
            ParticipateGlobalEventResult.Succeeded => TypedResults.Ok(new GlobalEventParticipationResponse { Participation = participation }),
            ParticipateGlobalEventResult.NotFound => TypedResults.NotFound(),
            ParticipateGlobalEventResult.NotRolledGame => TypedResults.BadRequest("The game isn't among the rolled ones"),
            ParticipateGlobalEventResult.NoGames => TypedResults.UnprocessableEntity("The player has unlocked an achievement of every game they could get"),
            _ => TypedResults.Conflict(ConflictText(result)),
        };

        // the platform of an automatic check names itself in the text
        public static string ConflictText(ParticipateGlobalEventResult result, AchievementPlatform? platform = null) => result switch
        {
            ParticipateGlobalEventResult.Finished => "The event is finished",
            ParticipateGlobalEventResult.AlreadyRolled => "The games are rolled already",
            ParticipateGlobalEventResult.NotRolled => "The games aren't rolled yet",
            ParticipateGlobalEventResult.AlreadyPicked => "A game is picked already",
            ParticipateGlobalEventResult.NotPicked => "No game is picked",
            ParticipateGlobalEventResult.TooEarly => "The picked game can't be refused yet",
            ParticipateGlobalEventResult.AlreadyPassed => "The picked game is passed already",
            ParticipateGlobalEventResult.ClaimPending => "A claim is under review",
            ParticipateGlobalEventResult.PlatformNotLinked => $"{platform} isn't linked",
            ParticipateGlobalEventResult.NoAchievements => "The game has no achievements, only a claim proves its passing",
            ParticipateGlobalEventResult.NoPlatformAchievements => $"The game has no achievements in {platform}",
            _ => "The roll was changed by another request",
        };
    }
}
