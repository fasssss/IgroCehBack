using Application.DTO;

namespace API.RepR.Response
{
    public class GetSteamAchievementsResponse
    {
        public List<PlatformAchievementObject> Achievements { get; set; }
    }
}
