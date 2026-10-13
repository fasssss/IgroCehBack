using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Post
{
    public class Logout : EndpointWithoutRequest<Ok>
    {
        public override void Configure()
        {
            Post("/api/logout");
            // user with an expired token must be able to log out too
            AllowAnonymous();
        }

        public override Task<Ok> ExecuteAsync(CancellationToken ct)
        {
            // cookie is HttpOnly, so only server can remove it; options have to match the ones it was issued with in AuthorizeByCode
            HttpContext.Response.Cookies.Delete("application_token", new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

            return Task.FromResult(TypedResults.Ok());
        }
    }
}
