using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using Infrastructure.Configurations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace API.Endpoints.Post
{
    public class PostAuthorizationCode: Endpoint<PostAuthorizationCodeRequest, Results<Ok, BadRequest>>
    {
        private readonly DiscordApiOptions _discordApiOptions;
        private readonly IAuthorizationApplicationService _authorizationApplicationService;

        public PostAuthorizationCode(
            IOptions<DiscordApiOptions> discordApiOptions,
            IAuthorizationApplicationService authorizationApplicationService)
        {
            _discordApiOptions = discordApiOptions.Value;
            _authorizationApplicationService = authorizationApplicationService;
        }

        public override void Configure()
        {
            Post("/api/postAuthorizationCode");
            AllowAnonymous();
        }

        public override async Task<Results<Ok, BadRequest>> ExecuteAsync(PostAuthorizationCodeRequest authCodeRequest, CancellationToken ct)
        {
            var authCode = authCodeRequest.AuthorizationCode;
            var data = await _authorizationApplicationService.Authorize(authCode);
            return TypedResults.Ok();
        }
    }
}
