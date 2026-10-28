using API.RepR.Request;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;

namespace API.Endpoints.Get
{
    // the image of a badge for an img tag on any page, so it needs no sign-in; a badge never changes, so it is cached for good
    public class GetBadgeImage : Endpoint<BadgeImageRequest, Results<FileContentHttpResult, NotFound>>
    {
        private readonly IBadgeApplicationService _badgeApplicationService;

        public GetBadgeImage(IBadgeApplicationService badgeApplicationService)
        {
            _badgeApplicationService = badgeApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getBadgeImage");
            AllowAnonymous();
        }

        public override async Task<Results<FileContentHttpResult, NotFound>> ExecuteAsync(BadgeImageRequest request, CancellationToken ct)
        {
            var image = await _badgeApplicationService.GetImageAsync(request.BadgeId);
            if (image == null)
            {
                return TypedResults.NotFound();
            }

            HttpContext.Response.Headers.CacheControl = "public, max-age=31536000, immutable";
            // an SVG opened by its link can't run scripts or load anything on the site of the API
            HttpContext.Response.Headers.ContentSecurityPolicy = "default-src 'none'; style-src 'unsafe-inline'; img-src data:";
            HttpContext.Response.Headers.XContentTypeOptions = "nosniff";
            return TypedResults.File(image.Content, image.ContentType);
        }
    }
}
