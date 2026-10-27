using Refit;

namespace Infrastructure.ExternalInterfaces
{
    // the sign-in through Steam, https://openid.net/specs/openid-authentication-2_0.html
    public interface ISteamCommunityApi
    {
        // Steam checks the signature of its own answer, the reply is "key:value" lines with is_valid:true or is_valid:false
        [Post("/openid/login")]
        public Task<ApiResponse<string>> CheckAuthentication([Body(BodySerializationMethod.UrlEncoded)] Dictionary<string, string> parameters, CancellationToken ct);
    }
}
