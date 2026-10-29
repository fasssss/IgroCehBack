using API.RepR.Response;
using Application.ApplicationInterfaces;
using FastEndpoints;

namespace API.Endpoints.Get
{
    // the page of OpenXBL that sends the user to the sign-in of Microsoft to link their Xbox account;
    // OpenXBL brings them back to the return address of its app, the front remembers the page they left
    public class GetXboxSignInLink : EndpointWithoutRequest<GetXboxSignInLinkResponse>
    {
        private readonly IXboxLinkApplicationService _xboxLinkApplicationService;

        public GetXboxSignInLink(IXboxLinkApplicationService xboxLinkApplicationService)
        {
            _xboxLinkApplicationService = xboxLinkApplicationService;
        }

        public override void Configure()
        {
            Get("/api/getXboxSignInLink");
        }

        public override Task<GetXboxSignInLinkResponse> ExecuteAsync(CancellationToken ct)
        {
            return Task.FromResult(new GetXboxSignInLinkResponse
            {
                XboxSignInLink = _xboxLinkApplicationService.GetSignInUrl(),
            });
        }
    }
}
