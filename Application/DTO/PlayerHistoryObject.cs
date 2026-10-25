using Domain.Enums;

namespace Application.DTO
{
    public class PlayerHistoryObject
    {
        public List<PlayerHistoryItemObject> Items { get; set; }
        public bool HasMore { get; set; }
    }

    // one event (season) of the player: the game they got and the game they guessed for someone else
    public class PlayerHistoryItemObject
    {
        public string EventId { get; set; }
        public string EventName { get; set; }
        public string GuildId { get; set; }
        public string GuildName { get; set; }
        public string GuildIconUrl { get; set; }
        public EventStatusId StatusId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public PlayerHistoryRecordObject ReceivedGame { get; set; }
        public PlayerHistoryRecordObject SuggestedGame { get; set; }
        // the player joined the active event and rolled a game of the pool, so they suggested nothing
        public bool IsNewcomer { get; set; }
    }

    public class PlayerHistoryRecordObject
    {
        public GameObject Game { get; set; }
        // who guessed the received game or who got the suggested one
        public PlayerShortObject Partner { get; set; }
        public int? Reward { get; set; }
        public DateTimeOffset? SucceededAt { get; set; }
    }

    // user data that is safe to show to other players
    public class PlayerShortObject
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string AvatarUrl { get; set; }
    }
}
