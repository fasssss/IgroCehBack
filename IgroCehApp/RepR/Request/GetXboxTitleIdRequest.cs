using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class GetXboxTitleIdRequest
    {
        // the product of the Microsoft Store the search found
        public string ProductId { get; set; }
    }

    public class GetXboxTitleIdValidator : Validator<GetXboxTitleIdRequest>
    {
        public GetXboxTitleIdValidator()
        {
            RuleFor(r => r.ProductId).Cascade(CascadeMode.Stop).NotEmpty().MaximumLength(XboxLimits.ProductIdMaxLength)
                .Matches("^[A-Za-z0-9]+$").WithMessage("A product id is letters and digits");
        }
    }
}
