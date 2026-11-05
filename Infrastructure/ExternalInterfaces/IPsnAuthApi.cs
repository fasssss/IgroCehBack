using Infrastructure.DTO;
using Refit;

namespace Infrastructure.ExternalInterfaces
{
    // the sign-in of the PlayStation App; the client of this API doesn't follow redirects
    public interface IPsnAuthApi
    {
        // the npsso cookie of the account gives a redirect to the app with the code in the address, or with the error of the sign-in
        [Get("/api/authz/v3/oauth/authorize")]
        public Task<HttpResponseMessage> Authorize([Header("Cookie")] string cookie, [AliasAs("access_type")] string accessType,
            [AliasAs("client_id")] string clientId, [AliasAs("redirect_uri")] string redirectUri, [AliasAs("response_type")] string responseType, string scope,
            CancellationToken ct);

        // the code of the sign-in or the refresh token is exchanged for tokens, 400 for a code or a token that doesn't work
        [Post("/api/authz/v3/oauth/token")]
        public Task<ApiResponse<PsnTokens>> GetTokens([Header("Authorization")] string clientAuthorization,
            [Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> form, CancellationToken ct);
    }
}
