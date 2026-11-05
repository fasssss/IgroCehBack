using Infrastructure.DTO;
using Refit;

namespace Infrastructure.ExternalInterfaces
{
    // the profiles live on a host of their own
    public interface IPsnProfileApi
    {
        // an account by its online id even if it isn't a friend of the service account, 404 when there is no such account
        [Get("/userProfile/v1/users/{onlineId}/profile2?fields=accountId,onlineId,aboutMe")]
        public Task<ApiResponse<PsnProfileResponse>> GetProfile([Header("Authorization")] string authorization, string onlineId, CancellationToken ct);
    }
}
