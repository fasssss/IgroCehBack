using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class JoinAsNewcomerRequest
    {
        public string EventId { get; set; }
        // the games of the pool the newcomer has passed already
        public List<string> ExcludedPoolGameIds { get; set; } = new();
    }

    public class JoinAsNewcomerValidator : Validator<JoinAsNewcomerRequest>
    {
        public JoinAsNewcomerValidator()
        {
            RuleFor(r => r.EventId).NotEmpty();
            RuleFor(r => r.ExcludedPoolGameIds).NotNull();
        }
    }
}
