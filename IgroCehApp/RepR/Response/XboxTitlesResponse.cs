using Application.DTO;

namespace API.RepR.Response
{
    public class SearchXboxTitlesResponse
    {
        public List<XboxTitleObject> Titles { get; set; }
    }

    public class GetXboxAchievementsResponse
    {
        // the name of the game in Xbox, so the administrator sees it is the right one
        public XboxTitleObject Title { get; set; }
        public List<XboxAchievementObject> Achievements { get; set; }
    }
}
