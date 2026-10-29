using Application.DTO;

namespace Application.Interfaces
{
    // the sign-in to Xbox through the app of OpenXBL proves that an Xbox account belongs to the user,
    // methods throw XboxUnavailableException when OpenXBL can't be reached
    public interface IXboxSignInService
    {
        // the page of OpenXBL that sends the user to Microsoft; OpenXBL brings them back to the return address set in its dashboard
        public string GetSignInUrl();
        // the account the code of the sign-in belongs to, null when OpenXBL doesn't accept the code (unknown, used or outdated)
        public Task<XboxAccountObject> ClaimAsync(string code, CancellationToken ct = default);
    }
}
