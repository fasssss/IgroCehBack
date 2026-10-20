namespace API.RepR.Request
{
    public class GetPlayerHistoryRequest
    {
        public string UserId { get; set; }
        // events of all shared guilds when empty
        public string GuildId { get; set; }
        public int StartFrom { get; set; }
    }
}
