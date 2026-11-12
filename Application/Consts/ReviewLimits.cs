namespace Application.Consts
{
    // the database columns and the validation of a published review share these limits
    public static class ReviewLimits
    {
        public const int MaxSections = 20;
        public const int TitleMaxLength = 100;
        public const int TextMaxLength = 5000;
        public const int ReplyMaxLength = 2000;
        // the score of the game: 1, 1.5, 2 … 10
        public const decimal MinScore = 1m;
        public const decimal MaxScore = 10m;
        public const decimal ScoreStep = 0.5m;
    }
}
