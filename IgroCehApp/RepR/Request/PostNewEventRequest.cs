namespace API.RepR.Request
{
    public class PostNewEventRequest
    {
        public string EventName { get; set; }
        public string GuildId { get; set; }
        public bool AllowsNewcomers { get; set; }
        // true plans the event for the next season, false puts it into the current one;
        // without it the last month of a season gives the next one
        public bool? IsNextSeason { get; set; }
    }
}
