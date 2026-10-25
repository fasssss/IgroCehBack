using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class AddNewcomerPoolGameRequest
    {
        public string EventId { get; set; }
        public string GameId { get; set; }
    }

    public class AddNewcomerPoolGameValidator : Validator<AddNewcomerPoolGameRequest>
    {
        public AddNewcomerPoolGameValidator()
        {
            RuleFor(r => r.EventId).NotEmpty();
            RuleFor(r => r.GameId).NotEmpty();
        }
    }
}
