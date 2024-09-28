using API.RepR.Response;
using FastEndpoints;

namespace API.Endpoints.Get
{
    public class GetDiscordAuthLink: EndpointWithoutRequest<GetDiscordAuthLinkResponse>
    {
        public GetDiscordAuthLink()
        {

        }

        public override void Configure()
        {
            Get("/api/getDiscordAuthLink");
            AllowAnonymous();
        }

        public override async Task<GetDiscordAuthLinkResponse> ExecuteAsync(CancellationToken ct)
        {
            return new GetDiscordAuthLinkResponse() 
            { 
            };
        }
    }
}
