using Application.DTO;
using Domain.Entities;
using Domain.Enums;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class PublishReviewRequest
    {
        public string EventRecordId { get; set; }
        // the thumb has to be chosen, so it can't fall back to false
        public bool? IsRecommended { get; set; }
        public ReviewVisibility? Visibility { get; set; }
        public List<NewReviewSectionObject> Sections { get; set; }
    }

    public class PublishReviewValidator : Validator<PublishReviewRequest>
    {
        public PublishReviewValidator()
        {
            RuleFor(r => r.IsRecommended).NotNull().WithMessage("Choose whether you recommend the game");
            RuleFor(r => r.Visibility).NotNull().IsInEnum();
            RuleFor(r => r.Sections).NotEmpty().WithMessage("A review needs at least one section")
                .Must(sections => sections.Count <= ReviewLimits.MaxSections).WithMessage($"A review can have up to {ReviewLimits.MaxSections} sections");
            RuleForEach(r => r.Sections).ChildRules(section =>
            {
                section.RuleFor(s => s.Title).Must(title => !string.IsNullOrWhiteSpace(title)).WithMessage("Every section needs a topic")
                    .MaximumLength(ReviewLimits.TitleMaxLength);
                section.RuleFor(s => s.Text).Must(text => !string.IsNullOrWhiteSpace(text)).WithMessage("Every section needs a text")
                    .MaximumLength(ReviewLimits.TextMaxLength);
            });
        }
    }
}
