using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    // the liked aspects of the current user in their order, they replace the saved ones
    public class SetLikedAspectsRequest
    {
        public List<int> AspectIds { get; set; }
    }

    public class SetLikedAspectsValidator : Validator<SetLikedAspectsRequest>
    {
        public SetLikedAspectsValidator()
        {
            RuleFor(r => r.AspectIds).Cascade(CascadeMode.Stop).NotNull()
                .Must(ids => ids.Count <= PreferenceLimits.MaxLikedAspects).WithMessage($"A player can like up to {PreferenceLimits.MaxLikedAspects} aspects")
                .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("An aspect is added once");
        }
    }
}
