namespace Application.DTO
{
    // guild where the user took part in at least one event
    public class PlayedGuildObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string IconUrl { get; set; }
        public int EventsPlayed { get; set; }
        public int EventsInProgress { get; set; }
    }
}
