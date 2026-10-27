using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class LinkSteamRequest
    {
        // the query of the page Steam sent the user to: returnPath, state and the openid.* fields of Steam
        public Dictionary<string, string> Parameters { get; set; }
    }

    public class LinkSteamValidator : Validator<LinkSteamRequest>
    {
        public LinkSteamValidator()
        {
            RuleFor(r => r.Parameters).NotNull();
        }
    }
}
