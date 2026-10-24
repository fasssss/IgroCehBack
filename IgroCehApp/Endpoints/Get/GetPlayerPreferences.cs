using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // what the player prefers and avoids, for those who see the profile
    public class GetPlayerPreferences : Endpoint<GetPlayerPreferencesRequest, Results<Ok<GetPlayerPreferencesResponse>, NotFound>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public GetPlayerPreferences(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getPlayerPreferences");
        }

        public override async Task<Results<Ok<GetPlayerPreferencesResponse>, NotFound>> ExecuteAsync(GetPlayerPreferencesRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var preferences = await _playerApplicationService.GetPlayerPreferencesAsync(stringId, request.UserId);
            if (preferences == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(new GetPlayerPreferencesResponse { Preferred = preferences.Preferred, Avoided = preferences.Avoided });
        }
    }
}
