using Domain.Enums;

namespace Application.DTO
{
    // a record of an event where the player guessed a game or got one, read for the statistics and the platforms of the profile
    public class PlayerRecordRow
    {
        public string EventId { get; set; }
        public string GuildId { get; set; }
        public EventStatusId EventStatusId { get; set; }
        public DateTime EventStartDate { get; set; }
        public string ParticipantId { get; set; }
        public string ToUserId { get; set; }
        public string GameId { get; set; }
        public List<string> GamePlatforms { get; set; }
        public int? Reward { get; set; }
        public DateTimeOffset? SucceededAt { get; set; }
        public bool IsNewcomer { get; set; }
        public bool IsEventFinished => EventStatusId == EventStatusId.Finished;
    }
}
