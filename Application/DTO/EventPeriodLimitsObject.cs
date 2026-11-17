namespace Application.DTO
{
    // the bounds of the own dates of an event as days, counted by the server (EventPeriodLimits);
    // the front only compares the days it picks with them
    public class EventPeriodLimitsObject
    {
        // today in UTC
        public DateOnly EarliestStart { get; set; }
        public DateOnly LatestStart { get; set; }
        // the start the bounds of the end are counted from
        public DateOnly StartDate { get; set; }
        public DateOnly EarliestEnd { get; set; }
        public DateOnly LatestEnd { get; set; }
        // the quick lengths from the start, the shortest and the longest among them
        public List<EventPeriodQuickEndObject> QuickEnds { get; set; }
        // for the texts of the front
        public int MinDays { get; set; }
        public int MaxMonths { get; set; }
        public int MaxStartMonths { get; set; }
    }
}
