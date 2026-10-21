namespace Application.DTO
{
    // rewards a player got for passed games in one guild, they count as soon as the passing is submitted
    public class RewardScore
    {
        public string GuildId { get; set; }
        public string UserId { get; set; }
        public int Score { get; set; }
    }
}
