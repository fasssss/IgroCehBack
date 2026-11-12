namespace Application.DTO
{
    // a favorite game of the player
    public class FavoriteGameObject
    {
        public GameObject Game { get; set; }
        // from the catalog, null for a game added by hand or a catalog game without the date
        public DateOnly? ReleaseDate { get; set; }
    }
}
