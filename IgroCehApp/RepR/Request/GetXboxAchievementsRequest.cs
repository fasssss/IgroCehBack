using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class GetXboxAchievementsRequest
    {
        // found by the search or typed by the administrator
        public string TitleId { get; set; }
    }

    public class GetXboxAchievementsValidator : Validator<GetXboxAchievementsRequest>
    {
        public GetXboxAchievementsValidator()
        {
            RuleFor(r => r.TitleId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(XboxLimits.TitleIdMaxLength)
                .Matches(@"^\d+$").WithMessage("A title id is a number");
        }
    }
}
