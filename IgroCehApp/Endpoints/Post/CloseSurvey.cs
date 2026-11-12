using API.Authorization;
using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the administrator of the site stops the active survey, after that no survey is active until a new one is made
    public class CloseSurvey : Endpoint<SurveyRequest, Results<Ok, NotFound, BadRequest<string>>>
    {
        private readonly ISurveyAdminApplicationService _surveyAdminApplicationService;

        public CloseSurvey(ISurveyAdminApplicationService surveyAdminApplicationService)
        {
            _surveyAdminApplicationService = surveyAdminApplicationService;
        }

        public override void Configure()
        {
            Post("/api/closeSurvey");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok, NotFound, BadRequest<string>>> ExecuteAsync(SurveyRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            switch (await _surveyAdminApplicationService.CloseAsync(stringId, request.SurveyId))
            {
                case ChangeSurveyResult.NotFound:
                    return TypedResults.NotFound();
                case ChangeSurveyResult.Unavailable:
                    return TypedResults.BadRequest("The survey is closed already");
            }

            return TypedResults.Ok();
        }
    }
}
