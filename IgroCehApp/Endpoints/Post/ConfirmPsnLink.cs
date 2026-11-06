using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // links the account with the online id given to StartPsnLink when its About me shows the code; 410 means the user has to start again
    public class ConfirmPsnLink : EndpointWithoutRequest<Results<Ok<LinkPsnResponse>, BadRequest<string>, NotFound<string>, StatusCodeHttpResult>>
    {
        private readonly IPsnLinkApplicationService _psnLinkApplicationService;

        public ConfirmPsnLink(IPsnLinkApplicationService psnLinkApplicationService)
        {
            _psnLinkApplicationService = psnLinkApplicationService;
        }

        public override void Configure()
        {
            Post("/api/confirmPsnLink");
        }

        public override async Task<Results<Ok<LinkPsnResponse>, BadRequest<string>, NotFound<string>, StatusCodeHttpResult>> ExecuteAsync(CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                var (result, onlineId) = await _psnLinkApplicationService.ConfirmAsync(stringId, ct);
                return result switch
                {
                    LinkPsnResult.NotStarted => TypedResults.StatusCode(StatusCodes.Status410Gone),
                    LinkPsnResult.CodeNotFound => TypedResults.BadRequest("About me doesn't show the code"),
                    LinkPsnResult.NotFound => TypedResults.NotFound("PSN has no account with this online id"),
                    LinkPsnResult.TooManyAttempts => TypedResults.StatusCode(StatusCodes.Status429TooManyRequests),
                    _ => TypedResults.Ok(new LinkPsnResponse { PsnOnlineId = onlineId }),
                };
            }
            catch (PsnUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
