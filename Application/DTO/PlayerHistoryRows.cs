using Domain.Enums;

namespace Application.DTO
{
    // an event of the history of the player as the query reads it, with every record of the player and of their partners;
    // PlayerApplicationService turns it into a PlayerHistoryItemObject
    public class HistoryEventRow
    {
        public string EventId { get; set; }
        public string EventName { get; set; }
        public string GuildId { get; set; }
        public string GuildName { get; set; }
        public string GuildIconUrl { get; set; }
        public EventStatusId StatusId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public List<HistoryRecordRow> Records { get; set; }
    }

    public class HistoryRecordRow
    {
        public string ParticipantId { get; set; }
        public string ParticipantUserName { get; set; }
        public string ParticipantAvatarUrl { get; set; }
        public string ToUserId { get; set; }
        public string ToUserName { get; set; }
        public string ToUserAvatarUrl { get; set; }
        public string GameId { get; set; }
        public string GameName { get; set; }
        public string GameImageUrl { get; set; }
        public string GameSteamUrl { get; set; }
        public int? Reward { get; set; }
        public DateTimeOffset? SucceededAt { get; set; }
        public bool IsNewcomer { get; set; }
    }
}
