using Application.DTO;
using Application.Enum;

namespace Application.ApplicationInterfaces
{
    // the badges the administrator uploads and gives for passing global events
    public interface IBadgeApplicationService
    {
        // every badge, the oldest first, with the global events that give it
        public Task<List<BadgeObject>> GetAllAsync();
        // the content type is one of BadgeLimits.ContentTypes, the content is checked against it
        public Task<(UploadBadgeResult Result, BadgeObject Badge)> UploadAsync(string userId, string name, string contentType, byte[] content);
        // null when there is no such badge
        public Task<BadgeImageObject> GetImageAsync(string badgeId);
    }
}
