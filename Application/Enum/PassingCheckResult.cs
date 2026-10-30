namespace Application.Enum
{
    // what a platform says about the picked game of a global event
    public enum PassingCheckResult
    {
        Passed = 0,
        // no chosen achievement is unlocked
        NotPassed = 1,
        // a chosen achievement was unlocked before the pick, it doesn't count
        UnlockedBeforePick = 2,
        // the platform hides the achievements of the player: the game details of the Steam profile, the privacy settings of Xbox
        ProfileHidden = 3,
    }
}
