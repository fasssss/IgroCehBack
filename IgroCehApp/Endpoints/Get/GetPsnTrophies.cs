using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the trophies of a game in PSN, the administrator picks the ones that mean the game is passed; 404 when it has none or there is no such game
    public class GetPsnTrophies : Endpoint<GetPsnTrophiesRequest, Results<Ok<GetPsnTrophiesResponse>, NotFound, StatusCodeHttpResult>>
    {
        private readonly IGlobalEventApplicationService _globalEventApplicationService;

        public GetPsnTrophies(IGlobalEventApplicationService globalEventApplicationService)
        {
            _globalEventApplicationService = globalEventApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getPsnTrophies");
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<GetPsnTrophiesResponse>, NotFound, StatusCodeHttpResult>> ExecuteAsync(GetPsnTrophiesRequest request, CancellationToken ct)
        {
            try
            {
                var title = await _globalEventApplicationService.GetPsnTrophiesAsync(request.TitleRef, ct);
                if (title == null)
                {
                    return TypedResults.NotFound();
                }

                return TypedResults.Ok(new GetPsnTrophiesResponse { TitleRef = title.TitleRef, TitleName = title.TitleName, Achievements = title.Achievements });
            }
            catch (PsnUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
