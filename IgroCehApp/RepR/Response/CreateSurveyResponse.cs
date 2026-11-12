using Application.DTO;

namespace API.RepR.Response
{
    public class CreateSurveyResponse
    {
        public SurveyObject Survey { get; set; }
        // the survey that was active until this one and is closed now, empty when there was none
        public string ClosedSurveyId { get; set; }
    }
}
