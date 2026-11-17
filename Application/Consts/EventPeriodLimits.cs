namespace Application.Consts
{
    // the own dates of an event, set in the advanced options of its creation instead of a season;
    // the validator of the creation and getEventPeriodLimits take the bounds from here, the front only compares the days it gets
    public static class EventPeriodLimits
    {
        public const int MinDays = 14;
        public const int MaxMonths = 6;
        // how far ahead the start can be planned
        public const int MaxStartMonths = 12;

        // the days are counted in UTC, as the seasons
        public static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);

        public static DateOnly LatestStart(DateOnly today) => today.AddMonths(MaxStartMonths);

        public static DateOnly EarliestEnd(DateOnly start) => start.AddDays(MinDays);

        public static DateOnly LatestEnd(DateOnly start) => start.AddMonths(MaxMonths);
    }
}
