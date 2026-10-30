namespace API.RepR.Request
{
    public class GetSteamAchievementsRequest
    {
        // the catalog id of the game (VgcId), a game outside Steam has no achievements there
        public int AppId { get; set; }
    }
}
