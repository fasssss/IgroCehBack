namespace API.RepR.Request
{
    public class GetPlayerReviewsRequest
    {
        public string UserId { get; set; }
        // reviews of all shared guilds when empty
        public string GuildId { get; set; }
    }
}
