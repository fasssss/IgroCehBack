using Infrastructure.DTO;
using Refit;

namespace Infrastructure.ExternalInterfaces
{
    // https://api.xbl.io/docs/json
    public interface IOpenXblApi
    {
        // the code the sign-in brought to the return address, exchanged for the account it belongs to; a code works once
        [Post("/app/claim")]
        public Task<ApiResponse<OpenXblClaim>> Claim([Body] OpenXblClaimRequest request, CancellationToken ct);
    }
}
