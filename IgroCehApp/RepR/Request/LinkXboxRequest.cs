using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class LinkXboxRequest
    {
        // the code from the query of the page OpenXBL sent the user to
        public string Code { get; set; }
    }

    public class LinkXboxValidator : Validator<LinkXboxRequest>
    {
        public LinkXboxValidator()
        {
            RuleFor(r => r.Code).NotEmpty().MaximumLength(XboxLimits.CodeMaxLength);
        }
    }
}
