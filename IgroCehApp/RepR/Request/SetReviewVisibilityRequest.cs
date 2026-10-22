using Domain.Enums;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class SetReviewVisibilityRequest
    {
        public string EventRecordId { get; set; }
        public ReviewVisibility? Visibility { get; set; }
    }

    public class SetReviewVisibilityValidator : Validator<SetReviewVisibilityRequest>
    {
        public SetReviewVisibilityValidator()
        {
            RuleFor(r => r.Visibility).NotNull().IsInEnum();
        }
    }
}
