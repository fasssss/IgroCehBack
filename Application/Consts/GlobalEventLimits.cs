namespace Application.Consts
{
    // the database columns and the validation of a global event share these limits
    public static class GlobalEventLimits
    {
        public const int NameMaxLength = 100;
        public const int DescriptionMaxLength = 2000;
        // the name of a badge image of the front without the extension
        public const int BadgeIdMaxLength = 100;
        public const string BadgeIdPattern = "^[A-Za-z0-9_-]+$";
        // a player rolls three games at once
        public const int MinGames = 3;
        public const int MaxGames = 50;
        public const int MaxAchievementsPerGame = 10;
        public const int AchievementTextMaxLength = 1000;
        // the games of one roll, the player picks one of them
        public const int RolledGames = 3;
        // the picked game can be refused for a new roll when this many days have passed since the pick
        public const int RefusalDelayDays = 7;
        // a claim: the profile the player types, the reason of a rejection the administrator writes
        public const int ClaimProfileMaxLength = 200;
        public const int ClaimCommentMaxLength = 500;
    }
}
