namespace Application.Enum
{
    public enum UpdateGlobalEventResult
    {
        Updated,
        NotFound,
        // a new game isn't in the catalog
        UnknownGame,
        // an achievement isn't among the achievements of its game in Steam
        UnknownAchievement,
        // a player has rolled the games already, they and their achievements can't change
        GamesLocked,
        // a player has passed the event and got its badge, it can't change
        BadgeLocked,
    }
}
