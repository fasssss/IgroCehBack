using Application.Enum;

namespace Application.DTO
{
    // the last automatic check of the passing on a platform and from when the next one there is allowed
    public class GlobalEventPassingCheckObject
    {
        public AchievementPlatform Platform { get; set; }
        public DateTimeOffset CheckedAt { get; set; }
        public DateTimeOffset CheckableAt { get; set; }
    }
}
