using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class SearchXboxTitlesRequest
    {
        // the name of the catalog game or what the administrator typed
        public string Query { get; set; }
    }

    public class SearchXboxTitlesValidator : Validator<SearchXboxTitlesRequest>
    {
        public SearchXboxTitlesValidator()
        {
            RuleFor(r => r.Query).NotEmpty().MaximumLength(XboxLimits.SearchQueryMaxLength);
        }
    }
}
