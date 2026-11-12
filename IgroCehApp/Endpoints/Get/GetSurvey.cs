using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // a survey with its questions for the administrator of the site, to change it
    public class GetSurvey : Endpoint<SurveyRequest, Results<Ok<GetSurveyResponse>, NotFound>>
    {
        private readonly ISurveyAdminApplicationService _surveyAdminApplicationService;

        public GetSurvey(ISurveyAdminApplicationService surveyAdminApplicationService)
        {
            _surveyAdminApplicationService = surveyAdminApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getSurvey");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<GetSurveyResponse>, NotFound>> ExecuteAsync(SurveyRequest request, CancellationToken ct)
        {
            var survey = await _surveyAdminApplicationService.GetAsync(request.SurveyId);
            if (survey == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new GetSurveyResponse { Survey = survey });
        }
    }
}
