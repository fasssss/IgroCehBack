namespace Domain.Entities
{
    // a game the player shows in "Favorite games" of the profile
    public class UserFavoriteGame
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public string GameId { get; set; }
        public Game Game { get; set; }
        // the games keep the order the player added them in
        public int Order { get; set; }
    }
}
