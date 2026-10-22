using Domain.Enums;

namespace Application.DTO
{
    // the page of a paired review; Review is empty until the player who passed the game publishes it
    public class ReviewPageObject
    {
        public string EventRecordId { get; set; }
        public string EventId { get; set; }
        public string EventName { get; set; }
        public string GuildId { get; set; }
        public GameObject Game { get; set; }
        // the player who passed the game and writes the review
        public PlayerShortObject Author { get; set; }
        // the player who guessed the game and answers the review
        public PlayerShortObject Guesser { get; set; }
        public bool IsPassed { get; set; }
        public ReviewObject Review { get; set; }
    }

    public class ReviewObject
    {
        public bool IsRecommended { get; set; }
        public ReviewVisibility Visibility { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public DateTimeOffset? RepliesPublishedAt { get; set; }
        public List<ReviewSectionObject> Sections { get; set; }
    }

    public class ReviewSectionObject
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
        public string Reply { get; set; }
    }

    // a section the player writes before publishing
    public class NewReviewSectionObject
    {
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class ReviewReplyObject
    {
        public string SectionId { get; set; }
        public string Text { get; set; }
    }

    // a review in the list on the profile of its author
    public class PlayerReviewObject
    {
        public string EventRecordId { get; set; }
        public string EventName { get; set; }
        public string GuildName { get; set; }
        public string GuildIconUrl { get; set; }
        public GameObject Game { get; set; }
        public PlayerShortObject Guesser { get; set; }
        public bool IsRecommended { get; set; }
        public ReviewVisibility Visibility { get; set; }
        public DateTimeOffset? PublishedAt { get; set; }
        public bool HasReplies { get; set; }
    }
}
