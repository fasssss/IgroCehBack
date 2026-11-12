using Application.DTO;

namespace API.RepR.Response
{
    public class GetFavoriteGamesResponse
    {
        // in the order the player added them
        public List<FavoriteGameObject> Games { get; set; }
    }
}
