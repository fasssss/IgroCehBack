using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    public class FindGameByName: Endpoint<FindGameByNameRequest, Results<Ok, BadRequest<string>>>
    {
        private readonly IAuthorizationApplicationService _authorizationApplicationService;

        public FindGameByName(IAuthorizationApplicationService authorizationApplicationService)
        {
            _authorizationApplicationService = authorizationApplicationService;
        }

        public override void Configure()
        {
            Get("/api/findGameByName");
        }

        public override async Task<Results<Ok, BadRequest<string>>> ExecuteAsync(FindGameByNameRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            if (stringId != null)
            {
                //var userObject = await _authorizationApplicationService.GetUserObjectAsync(stringId);
            }

            return TypedResults.Ok();
        }
    }
}
