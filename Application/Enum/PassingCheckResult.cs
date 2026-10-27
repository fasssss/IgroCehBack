namespace Application.Enum
{
    // what Steam says about the picked game of a global event
    public enum PassingCheckResult
    {
        Passed = 0,
        // no chosen achievement is unlocked
        NotPassed = 1,
        // a chosen achievement was unlocked before the pick, it doesn't count
        UnlockedBeforePick = 2,
        // the game details of the Steam profile are hidden, Steam doesn't give the achievements
        ProfileHidden = 3,
    }
}
