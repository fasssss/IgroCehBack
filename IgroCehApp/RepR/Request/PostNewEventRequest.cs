using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class PostNewEventRequest
    {
        public string EventName { get; set; }
        public string GuildId { get; set; }
        public bool AllowsNewcomers { get; set; }
        // true plans the event for the next season, false puts it into the current one;
        // without it the last month of a season gives the next one
        public bool? IsNextSeason { get; set; }
        // own dates of the event instead of a season, both or none; IsNextSeason doesn't count with them.
        // The event starts and ends at 00:00 UTC of these days, as a season does
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
    }

    public class PostNewEventValidator : Validator<PostNewEventRequest>
    {
        public PostNewEventValidator()
        {
            RuleFor(r => r.EndDate).NotNull().When(r => r.StartDate != null).WithMessage("An event with its own dates needs both the start and the end");
            RuleFor(r => r.StartDate).NotNull().When(r => r.EndDate != null).WithMessage("An event with its own dates needs both the start and the end");
            When(r => r.StartDate != null && r.EndDate != null, () =>
            {
                // a day of slack: the front may have got today as the earliest start just before midnight UTC
                RuleFor(r => r.StartDate)
                    .Must(start => start >= EventPeriodLimits.Today().AddDays(-1)).WithMessage("The event can't start in the past")
                    .Must(start => start <= EventPeriodLimits.LatestStart(EventPeriodLimits.Today()))
                    .WithMessage($"The start can be planned up to {EventPeriodLimits.MaxStartMonths} months ahead");
                RuleFor(r => r.EndDate)
                    .Must((r, end) => end >= EventPeriodLimits.EarliestEnd(r.StartDate!.Value))
                    .WithMessage($"An event lasts at least {EventPeriodLimits.MinDays} days")
                    .Must((r, end) => end <= EventPeriodLimits.LatestEnd(r.StartDate!.Value))
                    .WithMessage($"An event lasts up to {EventPeriodLimits.MaxMonths} months");
            });
        }
    }
}
