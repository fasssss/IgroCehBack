using API.Authorization;
using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the administrator of the site deletes a survey with its answers, the active one stops being active
    public class DeleteSurvey : Endpoint<SurveyRequest, Results<Ok, NotFound, Conflict<string>>>
    {
        private readonly ISurveyAdminApplicationService _surveyAdminApplicationService;

        public DeleteSurvey(ISurveyAdminApplicationService surveyAdminApplicationService)
        {
            _surveyAdminApplicationService = surveyAdminApplicationService;
        }

        public override void Configure()
        {
            Post("/api/deleteSurvey");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok, NotFound, Conflict<string>>> ExecuteAsync(SurveyRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            switch (await _surveyAdminApplicationService.DeleteAsync(stringId, request.SurveyId))
            {
                case ChangeSurveyResult.NotFound:
                    return TypedResults.NotFound();
                case ChangeSurveyResult.Unavailable:
                    return TypedResults.Conflict("The survey was closed at the same moment, it isn't deleted");
            }

            return TypedResults.Ok();
        }
    }
}
