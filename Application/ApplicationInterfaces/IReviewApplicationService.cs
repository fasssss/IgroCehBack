using Application.DTO;
using Application.Enum;
using Domain.Enums;

namespace Application.ApplicationInterfaces
{
    public interface IReviewApplicationService
    {
        public Task<ReviewPageObject> GetReviewPageAsync(string viewerId, string eventRecordId);
        public Task<PublishReviewResult> PublishReviewAsync(string userId, string eventRecordId, bool isRecommended, ReviewVisibility visibility, List<NewReviewSectionObject> sections);
        public Task<SetReviewVisibilityResult> SetVisibilityAsync(string userId, string eventRecordId, ReviewVisibility visibility);
        public Task<PublishReviewResult> PublishRepliesAsync(string userId, string eventRecordId, List<ReviewReplyObject> replies);
        public Task<List<PlayerReviewObject>> GetPlayerReviewsAsync(string viewerId, string playerId, string guildId);
    }
}
