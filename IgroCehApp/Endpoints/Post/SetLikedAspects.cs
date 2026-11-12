using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the current user saves what they like in games
    public class SetLikedAspects : Endpoint<SetLikedAspectsRequest, Results<Ok, BadRequest<string>>>
    {
        private readonly IPlayerApplicationService _playerApplicationService;

        public SetLikedAspects(IPlayerApplicationService playerApplicationService)
        {
            _playerApplicationService = playerApplicationService;
        }

        public override void Configure()
        {
            Post("/api/setLikedAspects");
        }

        public override async Task<Results<Ok, BadRequest<string>>> ExecuteAsync(SetLikedAspectsRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var result = await _playerApplicationService.SetLikedAspectsAsync(stringId, request.AspectIds);
            if (result == SetPreferencesResult.UnknownTag)
            {
                return TypedResults.BadRequest("There is no such aspect");
            }

            return TypedResults.Ok();
        }
    }
}
