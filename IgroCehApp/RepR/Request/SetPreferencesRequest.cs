using Domain.Entities;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    // both lists of the current user, they replace the saved ones
    public class SetPreferencesRequest
    {
        public List<int> PreferredTagIds { get; set; }
        public List<int> AvoidedTagIds { get; set; }
    }

    public class SetPreferencesValidator : Validator<SetPreferencesRequest>
    {
        public SetPreferencesValidator()
        {
            RuleFor(r => r.PreferredTagIds).Cascade(CascadeMode.Stop).NotNull()
                .Must(ids => ids.Count <= PreferenceLimits.MaxTagsPerList).WithMessage($"A list can have up to {PreferenceLimits.MaxTagsPerList} tags")
                .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("A tag is added once");
            RuleFor(r => r.AvoidedTagIds).Cascade(CascadeMode.Stop).NotNull()
                .Must(ids => ids.Count <= PreferenceLimits.MaxTagsPerList).WithMessage($"A list can have up to {PreferenceLimits.MaxTagsPerList} tags")
                .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("A tag is added once");
            RuleFor(r => r).Must(r => r.PreferredTagIds == null || r.AvoidedTagIds == null || !r.PreferredTagIds.Intersect(r.AvoidedTagIds).Any())
                .WithMessage("A tag can't be preferred and avoided at once");
        }
    }
}
