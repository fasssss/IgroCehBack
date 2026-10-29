using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.DTO;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the achievements of a game in Xbox, the administrator picks the ones that mean the game is passed; 404 when it has none or there is no such game
    public class GetXboxAchievements : Endpoint<GetXboxAchievementsRequest, Results<Ok<GetXboxAchievementsResponse>, NotFound, StatusCodeHttpResult>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public GetXboxAchievements(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getXboxAchievements");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<GetXboxAchievementsResponse>, NotFound, StatusCodeHttpResult>> ExecuteAsync(GetXboxAchievementsRequest request,
            CancellationToken ct)
        {
            try
            {
                var title = await _globalEventApplicationService.GetXboxAchievementsAsync(request.TitleId, ct);
                if (title == null)
                {
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok(new GetXboxAchievementsResponse
                {
                    Title = new XboxTitleObject { TitleId = title.TitleId, Name = title.Name },
                    Achievements = title.Achievements,
                });
            }
            catch (XboxUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
