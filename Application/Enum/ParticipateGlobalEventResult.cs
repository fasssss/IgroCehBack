namespace Application.Enum
{
    public enum ParticipateGlobalEventResult
    {
        Succeeded,
        NotFound,
        // the event is finished
        Finished,
        // the player has rolled the games of the event already
        AlreadyRolled,
        // the player hasn't rolled the games yet
        NotRolled,
        AlreadyPicked,
        // no game is picked to refuse
        NotPicked,
        // the game isn't among the rolled ones
        NotRolledGame,
        // the week since the pick hasn't passed yet
        TooEarly,
        // every game the player could get has a chosen achievement unlocked by them already
        NoGames,
        // another request of the player changed the roll first
        Conflict,
        // the picked game is passed, the participation is over
        AlreadyPassed,
        // a claim of the player is under review
        ClaimPending,
        // the automatic check on a platform needs the account of the player there linked
        PlatformNotLinked,
        // the picked game has no achievements, only a claim proves its passing
        NoAchievements,
        // the picked game has no achievements on the platform asked, the platform can't prove its passing
        NoPlatformAchievements,
        // the passing was checked on the platform less than GlobalEventLimits.PassingCheckDelayMinutes ago
        CheckedRecently,
    }
}
