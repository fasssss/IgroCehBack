namespace Domain.Entities
{
    public class ReviewSection : IBaseEntity
    {
        public string Id { get; set; }
        public string ReviewId { get; set; }
        public Review Review { get; set; }
        // sections are shown in the order they were written
        public int Order { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        // the answer of the one who guessed the game, a section may stay without it
        public string? Reply { get; set; }
        public DateTimeOffset? CreatedAt { get; set; }
    }
}
