using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    // the survey comes in full, the questions in their new order
    public class UpdateSurveyRequest : CreateSurveyRequest
    {
        public string SurveyId { get; set; }
    }

    public class UpdateSurveyValidator : Validator<UpdateSurveyRequest>
    {
        public UpdateSurveyValidator()
        {
            RuleFor(r => r.SurveyId).NotEmpty();
            // the same title, description and questions as on the creation
            Include(new CreateSurveyValidator());
        }
    }
}
