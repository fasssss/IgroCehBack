using Application.DTO;

namespace API.RepR.Response
{
    public class GetPlayerGamesResponse
    {
        public List<PlayerGameObject> Games { get; set; }
        public bool HasMore { get; set; }
    }
}
