namespace Application.DTO
{
    // a global event that gives a badge for its passing, the administrator sees it when picking a badge for another event
    public class GlobalEventBadgeUseObject
    {
        public string BadgeId { get; set; }
        public string GlobalEventId { get; set; }
        public string GlobalEventName { get; set; }
        // the players who got the badge for this event
        public int PassedCount { get; set; }
    }
}
