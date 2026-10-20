namespace API.RepR.Request
{
    public class GetPlayerProfileRequest
    {
        public string UserId { get; set; }
        // statistics of all shared guilds when empty
        public string GuildId { get; set; }
    }
}
