using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    public class UnlinkPsn : EndpointWithoutRequest<Ok>
    {
        private readonly IPsnLinkApplicationService _psnLinkApplicationService;

        public UnlinkPsn(IPsnLinkApplicationService psnLinkApplicationService)
        {
            _psnLinkApplicationService = psnLinkApplicationService;
        }

        public override void Configure()
        {
            Post("/api/unlinkPsn");
        }

        public override async Task<Ok> ExecuteAsync(CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            await _psnLinkApplicationService.UnlinkAsync(stringId);
            return TypedResults.Ok();
        }
    }
}
