using Domain.Enums;

namespace Domain.Entities
{
    // a paired review: the player who passed the game writes it, the one who guessed the game answers its sections;
    // both are published once and can't be changed after that
    public class Review : IBaseEntity
    {
        public string Id { get; set; }
        public string EventRecordId { get; set; }
        public EventRecord EventRecord { get; set; }
        // the thumb up or down of the player who passed the game
        public bool IsRecommended { get; set; }
        // the author changes it at any time, the texts stay as published
        public ReviewVisibility Visibility { get; set; }
        public List<ReviewSection> Sections { get; set; }
        // the time of publishing, the review is published as soon as it is written
        public DateTimeOffset? CreatedAt { get; set; }
        // the one who guessed the game publishes all the answers at once
        public DateTimeOffset? RepliesPublishedAt { get; set; }
    }
}
