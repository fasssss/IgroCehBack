using Application.DTO;

namespace API.RepR.Response
{
    public class GetSurveysResponse
    {
        // every survey, the latest first
        public List<SurveyShortObject> Surveys { get; set; }
    }
}
