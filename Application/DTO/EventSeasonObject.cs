namespace Application.DTO
{
    public class EventSeasonObject
    {
        public DateTimeOffset Start { get; set; }
        // the start of the following season, as Event.EndDate
        public DateTimeOffset End { get; set; }
    }
}
