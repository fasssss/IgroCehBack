using Application.DTO;

namespace API.RepR.Response
{
    public class GetSteamAchievementsResponse
    {
        public List<SteamAchievementObject> Achievements { get; set; }
    }
}
