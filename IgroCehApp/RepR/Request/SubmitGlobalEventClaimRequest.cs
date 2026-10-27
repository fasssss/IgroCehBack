using Application.Consts;
using Application.Enum;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class SubmitGlobalEventClaimRequest
    {
        public string GlobalEventId { get; set; }
        public ClaimPlatform? Platform { get; set; }
        // a name or a link of the profile that shows the passing
        public string Profile { get; set; }
    }

    public class SubmitGlobalEventClaimValidator : Validator<SubmitGlobalEventClaimRequest>
    {
        public SubmitGlobalEventClaimValidator()
        {
            RuleFor(r => r.Platform).NotNull().IsInEnum();
            RuleFor(r => r.Profile).Cascade(CascadeMode.Stop).NotNull().Must(profile => !string.IsNullOrWhiteSpace(profile))
                .Must(profile => profile.Trim().Length <= GlobalEventLimits.ClaimProfileMaxLength);
        }
    }
}
