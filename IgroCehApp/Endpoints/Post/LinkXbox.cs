using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using Application.Enum;
using Application.Exceptions;
using FastEndpoints;
using Microsoft.AspNetCore.Http.HttpResults;
using System.Security.Claims;

namespace API.Endpoints.Post
{
    // the front passes the code the sign-in brought back, OpenXBL exchanges it for the account, then the account is linked
    public class LinkXbox : Endpoint<LinkXboxRequest, Results<Ok<LinkXboxResponse>, BadRequest<string>, Conflict<string>, StatusCodeHttpResult>>
    {
        private readonly IXboxLinkApplicationService _xboxLinkApplicationService;

        public LinkXbox(IXboxLinkApplicationService xboxLinkApplicationService)
        {
            _xboxLinkApplicationService = xboxLinkApplicationService;
        }

        public override void Configure()
        {
            Post("/api/linkXbox");
        }

        public override async Task<Results<Ok<LinkXboxResponse>, BadRequest<string>, Conflict<string>, StatusCodeHttpResult>> ExecuteAsync(
            LinkXboxRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            try
            {
                var (result, gamertag) = await _xboxLinkApplicationService.LinkAsync(stringId, request.Code, ct);
                return result switch
                {
                    LinkXboxResult.InvalidCode => TypedResults.BadRequest("Xbox doesn't confirm the sign-in"),
                    LinkXboxResult.TakenByAnotherUser => TypedResults.Conflict("The Xbox account is linked to another user"),
                    _ => TypedResults.Ok(new LinkXboxResponse { XboxGamertag = gamertag }),
                };
            }
            catch (XboxUnavailableException)
            {
                return TypedResults.StatusCode(StatusCodes.Status503ServiceUnavailable);
            }
        }
    }
}
