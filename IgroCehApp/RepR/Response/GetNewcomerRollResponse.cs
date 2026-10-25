using Application.DTO;

namespace API.RepR.Response
{
    public class GetNewcomerRollResponse
    {
        public List<NewcomerPoolGameObject> Games { get; set; }
        public int HiddenCount { get; set; }
        public int MinGames { get; set; }
    }
}
