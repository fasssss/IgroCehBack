namespace API.RepR.Request
{
    public class PostNewEventRequest
    {
        public string EventName { get; set; }
        public string GuildId { get; set; }
        public bool AllowsNewcomers { get; set; }
    }
}
