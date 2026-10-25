namespace Domain.Entities
{
    public static class NewcomerPoolLimits
    {
        // how many games one participant puts into the pool
        public const int GamesPerPlayer = 1;
        // the event leaves the registration with at least this many games in the pool, and a newcomer keeps at least this many to roll from
        public const int MinGames = 3;
    }
}
