namespace Application.DTO
{
    public class PlayerGamesObject
    {
        public List<PlayerGameObject> Games { get; set; }
        public bool HasMore { get; set; }
    }

    // a game of the tab "Мои игры": passed by the player or guessed by them for someone else
    public class PlayerGameObject
    {
        public string EventRecordId { get; set; }
        public GameObject Game { get; set; }
        // who guessed a passed game or who got a guessed one
        public PlayerShortObject Partner { get; set; }
        public string EventId { get; set; }
        public string EventName { get; set; }
        // the time of the passing or of the guess
        public DateTimeOffset? Date { get; set; }
        // the review of a passed game and its score, when the viewer may read it
        public string ReviewId { get; set; }
        public decimal? Score { get; set; }
    }
}
