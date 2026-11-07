using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class GetPsnTrophiesRequest
    {
        // typed by the administrator: the id of the trophy list, e.g. NPWR20188_00, the service after it is optional
        public string TitleRef { get; set; }
    }

    public class GetPsnTrophiesValidator : Validator<GetPsnTrophiesRequest>
    {
        public GetPsnTrophiesValidator()
        {
            RuleFor(r => r.TitleRef).Must(titleRef => PsnLimits.TryParseTitleRef(titleRef, out _, out _)).WithMessage("The id of a trophy list looks like NPWR20188_00");
        }
    }
}
