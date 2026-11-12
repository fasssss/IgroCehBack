using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Post
{
    // the administrator of the site changes the active survey: everything until somebody answers, then only what isn't a question
    public class UpdateSurvey : Endpoint<UpdateSurveyRequest, Results<Ok<GetSurveyResponse>, NotFound, Conflict<string>>>
    {
        private readonly ISurveyAdminApplicationService _surveyAdminApplicationService;

        public UpdateSurvey(ISurveyAdminApplicationService surveyAdminApplicationService)
        {
            _surveyAdminApplicationService = surveyAdminApplicationService;
        }

        public override void Configure()
        {
            Post("/api/updateSurvey");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<GetSurveyResponse>, NotFound, Conflict<string>>> ExecuteAsync(UpdateSurveyRequest request, CancellationToken ct)
        {
            var (result, survey) = await _surveyAdminApplicationService.UpdateAsync(request.SurveyId, request.Title, request.Description,
                request.ShowsResultsToRespondents, request.Questions);
            switch (result)
            {
                case UpdateSurveyResult.NotFound:
                    return TypedResults.NotFound();
                case UpdateSurveyResult.Closed:
                    return TypedResults.Conflict("The survey is closed, it can't change");
                case UpdateSurveyResult.QuestionsLocked:
                    return TypedResults.Conflict("Users have answered the survey, its questions can't change");
                case UpdateSurveyResult.ChangedMeanwhile:
                    return TypedResults.Conflict("The survey was closed or answered at the same moment, it isn't saved");
            }

            return TypedResults.Ok(new GetSurveyResponse { Survey = survey });
        }
    }
}
