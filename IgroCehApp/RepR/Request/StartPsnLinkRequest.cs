using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class StartPsnLinkRequest
    {
        public string OnlineId { get; set; }
    }

    public class StartPsnLinkValidator : Validator<StartPsnLinkRequest>
    {
        public StartPsnLinkValidator()
        {
            RuleFor(r => r.OnlineId).Must(PsnLimits.IsOnlineId);
        }
    }
}
