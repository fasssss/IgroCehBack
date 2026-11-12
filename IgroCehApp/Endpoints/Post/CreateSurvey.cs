using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the administrator of the site publishes a survey, it becomes the active one and closes the one before it
    public class CreateSurvey : Endpoint<CreateSurveyRequest, Results<Ok<CreateSurveyResponse>, Conflict<string>>>
    {
        private readonly ISurveyAdminApplicationService _surveyAdminApplicationService;

        public CreateSurvey(ISurveyAdminApplicationService surveyAdminApplicationService)
        {
            _surveyAdminApplicationService = surveyAdminApplicationService;
        }

        public override void Configure()
        {
            Post("/api/createSurvey");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<CreateSurveyResponse>, Conflict<string>>> ExecuteAsync(CreateSurveyRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var (result, survey, closedSurveyId) = await _surveyAdminApplicationService.CreateAsync(stringId, request.Title, request.Description,
                request.ShowsResultsToRespondents, request.Questions);
            if (result == CreateSurveyResult.ChangedMeanwhile)
            {
                return TypedResults.Conflict("The surveys were changed at the same moment, the survey isn't published");
            }

            return TypedResults.Ok(new CreateSurveyResponse { Survey = survey, ClosedSurveyId = closedSurveyId });
        }
    }
}
