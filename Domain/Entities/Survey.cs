namespace Domain.Entities
{
    // a survey of the whole site made by its administrator: it is published at once and stays active until the next one is made
    // or the administrator stops it; every signed-in user answers the active one once
    public class Survey : IBaseEntity
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        // the respondents see how the community answered the questions with options, without names
        public bool ShowsResultsToRespondents { get; set; }
        public string CreatedById { get; set; }
        public User CreatedBy { get; set; }
        public List<SurveyQuestion> Questions { get; set; } = new List<SurveyQuestion>();
        // once there is one, the questions and their options can't change
        public List<SurveyResponse> Responses { get; set; } = new List<SurveyResponse>();
        public DateTimeOffset? CreatedAt { get; set; }
        // true while the survey is active and empty once it is closed: the unique index lets one survey be active,
        // the empty ones of the closed surveys don't count in it
        public bool? IsActive { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public string? ClosedById { get; set; }
        // soft delete, like the events: the query filter of the context hides it with its answers
        public bool IsDeleted { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }
        public string? DeletedById { get; set; }
    }
}
