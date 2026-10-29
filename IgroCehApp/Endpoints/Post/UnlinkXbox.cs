using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class UnlinkXbox : EndpointWithoutRequest<Ok>
    {
        private readonly IXboxLinkApplicationService _xboxLinkApplicationService;

        public UnlinkXbox(IXboxLinkApplicationService xboxLinkApplicationService)
        {
            _xboxLinkApplicationService = xboxLinkApplicationService;
        }

        public override void Configure()
        {
            Post("/api/unlinkXbox");
        }

        public override async Task<Ok> ExecuteAsync(CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            await _xboxLinkApplicationService.UnlinkAsync(stringId);
            return TypedResults.Ok();
        }
    }
}
