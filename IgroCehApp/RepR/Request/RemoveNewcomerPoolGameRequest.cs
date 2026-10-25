using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class RemoveNewcomerPoolGameRequest
    {
        public string EventId { get; set; }
        public string PoolGameId { get; set; }
    }

    public class RemoveNewcomerPoolGameValidator : Validator<RemoveNewcomerPoolGameRequest>
    {
        public RemoveNewcomerPoolGameValidator()
        {
            RuleFor(r => r.EventId).NotEmpty();
            RuleFor(r => r.PoolGameId).NotEmpty();
        }
    }
}
