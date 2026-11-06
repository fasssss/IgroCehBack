using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // gives the code the user puts in About me to prove the account with this online id, ConfirmPsnLink checks it after that; PSN isn't asked here
    public class StartPsnLink : Endpoint<StartPsnLinkRequest, Ok<StartPsnLinkResponse>>
    {
        private readonly IPsnLinkApplicationService _psnLinkApplicationService;

        public StartPsnLink(IPsnLinkApplicationService psnLinkApplicationService)
        {
            _psnLinkApplicationService = psnLinkApplicationService;
        }

        public override void Configure()
        {
            Post("/api/startPsnLink");
        }

        public override Task<Ok<StartPsnLinkResponse>> ExecuteAsync(StartPsnLinkRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var code = _psnLinkApplicationService.Start(stringId, request.OnlineId);
            return Task.FromResult(TypedResults.Ok(new StartPsnLinkResponse { Code = code }));
        }
    }
}
