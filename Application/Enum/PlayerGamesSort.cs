namespace Application.Enum
{
    public enum PlayerGamesSort
    {
        // the latest passing or guess first
        Latest = 0,
        // the passed games with a score the viewer may read first, the best one on top; the guessed games are always the latest first
        Score = 1,
    }
}
