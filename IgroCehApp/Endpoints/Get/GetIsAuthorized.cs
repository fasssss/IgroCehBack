using Application.DTO;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    public class GetIsAuthorized: EndpointWithoutRequest<Ok<bool>>
    {
        public override void Configure()
        {
            Get("/api/getIsAuthorized");
        }

        public override async Task<Ok<bool>> ExecuteAsync(CancellationToken ct)
        {
            var accessToken = HttpContext.Request.Cookies["access_token"];
            if (accessToken != null)
            {
                return TypedResults.Ok(true);
            }

            return TypedResults.Ok(false);
        }
    }
}
