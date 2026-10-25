using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class ReplaceNewcomerPoolGameRequest
    {
        public string EventId { get; set; }
        public string PoolGameId { get; set; }
        public string GameId { get; set; }
    }

    public class ReplaceNewcomerPoolGameValidator : Validator<ReplaceNewcomerPoolGameRequest>
    {
        public ReplaceNewcomerPoolGameValidator()
        {
            RuleFor(r => r.EventId).NotEmpty();
            RuleFor(r => r.PoolGameId).NotEmpty();
            RuleFor(r => r.GameId).NotEmpty();
        }
    }
}
