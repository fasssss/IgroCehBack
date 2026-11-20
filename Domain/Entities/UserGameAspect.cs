namespace Domain.Entities
{
    // an aspect of games the player likes
    public class UserGameAspect
    {
        public string UserId { get; set; }
        public User User { get; set; }
        public int GameAspectId { get; set; }
        public GameAspect GameAspect { get; set; }
        // the aspects keep the order the player added them in
        public int Order { get; set; }
    }
}
