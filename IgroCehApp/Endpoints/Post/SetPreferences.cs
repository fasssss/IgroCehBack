using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the current user saves what they prefer and avoid
    public class SetPreferences : Endpoint<SetPreferencesRequest, Results<Ok, BadRequest<string>>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public SetPreferences(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Post("/api/setPreferences");
        }

        public override async Task<Results<Ok, BadRequest<string>>> ExecuteAsync(SetPreferencesRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _playerApplicationService.SetPreferencesAsync(stringId, request.PreferredTagIds, request.AvoidedTagIds);
            if (result == SetPreferencesResult.UnknownTag)
            {
                return TypedResults.BadRequest("There is no such tag");
            }

            return TypedResults.Ok();
        }
    }
}
