namespace API.RepR.Request
{
    public class GetSteamAchievementsRequest
    {
        // the Steam app id, the same as the catalog id of the game (VgcId)
        public int AppId { get; set; }
    }
}
