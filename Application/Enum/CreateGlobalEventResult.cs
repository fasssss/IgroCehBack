namespace Application.Enum
{
    public enum CreateGlobalEventResult
    {
        Created,
        // one global event is active at a time
        AlreadyActive,
        // a game isn't in the catalog
        UnknownGame,
        // an achievement isn't among the achievements of its game in Steam
        UnknownAchievement,
        // there is no such badge
        UnknownBadge,
    }
}
