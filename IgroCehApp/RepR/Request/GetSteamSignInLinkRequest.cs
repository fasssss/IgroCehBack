using API.Helpers;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class GetSteamSignInLinkRequest
    {
        // the page of the front the user comes back to after Steam
        public string ReturnPath { get; set; }
    }

    public class GetSteamSignInLinkValidator : Validator<GetSteamSignInLinkRequest>
    {
        public GetSteamSignInLinkValidator()
        {
            RuleFor(r => r.ReturnPath).Must(SteamLinkState.IsLocalPath).WithMessage("The return path is a path of the site")
                .MaximumLength(500);
        }
    }
}
