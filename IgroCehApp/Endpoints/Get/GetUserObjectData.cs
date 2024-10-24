using API.RepR.Request;
using Application.ApplicationInterfaces;
using Application.DTO;
using FastEndpoints;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;

namespace API.Endpoints.Get
{
    public class GetUserObjectData: EndpointWithoutRequest<Results<Ok<UserObject>, BadRequest<string>>>
    {
        private readonly IAuthorizationApplicationService _authorizationApplicationService;

        public GetUserObjectData(IAuthorizationApplicationService authorizationApplicationService)
        {
            _authorizationApplicationService = authorizationApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getUserObject");
        }

        public override async Task<Results<Ok<UserObject>, BadRequest<string>>> ExecuteAsync(CancellationToken ct)
        {
            var stringId = HttpContext.Request.Cookies["id"];
            if(long.TryParse(stringId, out long id))
            {
                var userObject = await _authorizationApplicationService.GetUserObjectAsync(id);
                return TypedResults.Ok(userObject);
            }

            return TypedResults.BadRequest("Couldn't retrieve user data");
        }
    }
}
