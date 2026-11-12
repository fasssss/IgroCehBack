namespace Application.Enum
{
    public enum SetFavoriteGamesResult
    {
        Saved,
        // a game isn't saved in the database, a catalog game is saved by createGameFromVgc first
        UnknownGame,
    }
}
