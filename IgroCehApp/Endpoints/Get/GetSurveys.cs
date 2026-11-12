using API.Authorization;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // every survey for the administrator of the site: the active one and the closed ones, which nobody else sees
    public class GetSurveys : EndpointWithoutRequest<Ok<GetSurveysResponse>>
    {
        private readonly ISurveyAdminApplicationService _surveyAdminApplicationService;

        public GetSurveys(ISurveyAdminApplicationService surveyAdminApplicationService)
        {
            _surveyAdminApplicationService = surveyAdminApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getSurveys");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Ok<GetSurveysResponse>> ExecuteAsync(CancellationToken ct)
        {
            return TypedResults.Ok(new GetSurveysResponse { Surveys = await _surveyAdminApplicationService.GetAllAsync() });
        }
    }
}
