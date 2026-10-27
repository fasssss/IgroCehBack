using API.Helpers;
using API.RepR.Request;
using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;
using System.Security.Claims;

namespace API.Endpoints.Get
{
    // the page of Steam where the user signs in to link their Steam account, Steam sends them back to the front
    public class GetSteamSignInLink : Endpoint<GetSteamSignInLinkRequest, GetSteamSignInLinkResponse>
    {
        private readonly ISteamLinkApplicationService _steamLinkApplicationService;
        private readonly SteamLinkState _steamLinkState;

        public GetSteamSignInLink(ISteamLinkApplicationService steamLinkApplicationService, SteamLinkState steamLinkState)
        {
            _steamLinkApplicationService = steamLinkApplicationService;
            _steamLinkState = steamLinkState;
        }

        public override void Configure()
        {
            Get("/api/getSteamSignInLink");
        }

        public override Task<GetSteamSignInLinkResponse> ExecuteAsync(GetSteamSignInLinkRequest request, CancellationToken ct)
        {
            var stringId = HttpContext.User.Claims.Single(c => c.Type == ClaimTypes.NameIdentifier).Value;
            var returnTo = _steamLinkState.CreateReturnTo(stringId, request.ReturnPath);
            return Task.FromResult(new GetSteamSignInLinkResponse
            {
                SteamSignInLink = _steamLinkApplicationService.GetSignInUrl(returnTo, _steamLinkState.Realm),
            });
        }
    }
}
