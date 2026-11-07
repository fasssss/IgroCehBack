using Application.DTO;

namespace API.RepR.Response
{
    public class GetPsnTrophiesResponse
    {
        // the game in PSN with the service that keeps its list, the event is saved with this ref
        public string TitleRef { get; set; }
        // the name of the game in PSN, so the administrator sees it is the right one
        public string TitleName { get; set; }
        public List<PlatformAchievementObject> Achievements { get; set; }
    }
}
