using Application.Enum;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class CheckGlobalEventPassingRequest : GlobalEventRequest
    {
        // where the achievements are checked; Steam when it isn't given, as the front sent it before Xbox
        public AchievementPlatform Platform { get; set; } = AchievementPlatform.Steam;
    }

    public class CheckGlobalEventPassingValidator : Validator<CheckGlobalEventPassingRequest>
    {
        public CheckGlobalEventPassingValidator()
        {
            RuleFor(r => r.GlobalEventId).NotEmpty();
            RuleFor(r => r.Platform).IsInEnum();
        }
    }
}
