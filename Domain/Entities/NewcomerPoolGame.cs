namespace Domain.Entities
{
    // a game a participant puts into the pool of the event during the registration;
    // a newcomer who joins the active event rolls one game of the pool
    public class NewcomerPoolGame : IBaseEntity
    {
        public string Id { get; set; }
        public string EventId { get; set; }
        public Event Event { get; set; }
        public string GameId { get; set; }
        public Game Game { get; set; }
        // the participant who put the game into the pool, the newcomers don't see them
        public string AuthorId { get; set; }
        public User Author { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
