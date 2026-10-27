using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the achievements of a Steam game, the administrator picks the ones that mean the game is passed
    public class GetSteamAchievements : Endpoint<GetSteamAchievementsRequest, Results<Ok<GetSteamAchievementsResponse>, StatusCodeHttpResult>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public GetSteamAchievements(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getSteamAchievements");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<GetSteamAchievementsResponse>, StatusCodeHttpResult>> ExecuteAsync(GetSteamAchievementsRequest request, CancellationToken ct)
        {
            try
            {
                var achievements = await _globalEventApplicationService.GetGameAchievementsAsync(request.AppId, ct);
                return TypedResults.Ok(new GetSteamAchievementsResponse { Achievements = achievements });
            }
            catch (SteamUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
