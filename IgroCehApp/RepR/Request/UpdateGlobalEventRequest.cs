using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    // the event comes in full, the games in their new order; the image is replaced with its own request
    public class UpdateGlobalEventRequest : CreateGlobalEventRequest
    {
        public string GlobalEventId { get; set; }
    }

    public class UpdateGlobalEventValidator : Validator<UpdateGlobalEventRequest>
    {
        public UpdateGlobalEventValidator()
        {
            RuleFor(r => r.GlobalEventId).NotEmpty();
            // the same name, description and games as on the creation
            Include(new CreateGlobalEventValidator());
        }
    }
}
