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
    }
}
