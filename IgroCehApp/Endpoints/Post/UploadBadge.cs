using API.Authorization;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the administrator of the site adds a badge to give for passing global events; it is kept in the database and never replaced
    public class UploadBadge : Endpoint<UploadBadgeRequest, Results<Ok<UploadBadgeResponse>, BadRequest<string>>>
    {
        private readonly IBadgeApplicationService _badgeApplicationService;

        public UploadBadge(IBadgeApplicationService badgeApplicationService)
        {
            _badgeApplicationService = badgeApplicationService;
        }

        public override void Configure()
        {
            Post("/api/uploadBadge");
            AllowFileUploads();
            Policies(AppAdminPolicy.Name);
        }

        public override async Task<Results<Ok<UploadBadgeResponse>, BadRequest<string>>> ExecuteAsync(UploadBadgeRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            using var content = new MemoryStream();
            await request.Image.CopyToAsync(content, ct);
            var (result, badge) = await _badgeApplicationService.UploadAsync(stringId, request.Name, request.Image.ContentType, content.ToArray());
            if (result == UploadBadgeResult.InvalidImage)
            {
                return TypedResults.BadRequest("The image isn't a safe SVG, a PNG or a WebP");
            }

            return TypedResults.Ok(new UploadBadgeResponse { Badge = badge });
        }
    }
}
