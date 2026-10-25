using Application.DTO;

namespace API.RepR.Response
{
    public class GetNewcomerPoolResponse
    {
        public List<NewcomerPoolGameObject> Games { get; set; }
        public int MinGames { get; set; }
        public int GamesPerPlayer { get; set; }
    }
}
