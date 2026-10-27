namespace Application.Consts
{
    // the database columns and the validation of a global event share these limits
    public static class GlobalEventLimits
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 2000;
        // a player rolls three games at once
        public const int MinGames = 3;
        public const int MaxGames = 50;
        public const int MaxAchievementsPerGame = 10;
        public const int AchievementTextMaxLength = 1000;
    }
}
