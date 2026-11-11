namespace Application.DTO
{
    // the seasons a new event can go to; the server counts them, so the front shows the same dates the event gets
    public class EventSeasonsObject
    {
        public EventSeasonObject Current { get; set; }
        public EventSeasonObject Next { get; set; }
        // the last month of a season offers the next one
        public bool IsNextByDefault { get; set; }
    }
}
