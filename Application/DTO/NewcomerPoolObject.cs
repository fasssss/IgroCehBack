namespace Application.DTO
{
    // the pool of an event as a viewer sees it: the authors stay hidden, a participant knows which games are theirs
    public class NewcomerPoolObject
    {
        public List<NewcomerPoolGameObject> Games { get; set; }
        // the event leaves the registration with this many games
        public int MinGames { get; set; }
        public int GamesPerPlayer { get; set; }
    }

    // what may drop to a newcomer who joins the active event
    public class NewcomerRollObject
    {
        public List<NewcomerPoolGameObject> Games { get; set; }
        // the games of the pool the newcomer has already got somewhere, they don't drop again
        public int HiddenCount { get; set; }
        // the newcomer crosses out games, but keeps at least this many
        public int MinGames { get; set; }
    }

    public class NewcomerPoolGameObject
    {
        public string Id { get; set; }
        public GameObject Game { get; set; }
        public bool IsMine { get; set; }
    }
}
