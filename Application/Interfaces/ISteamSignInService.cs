namespace Application.Interfaces
{
    // the sign-in through Steam (OpenID 2.0) proves that a Steam account belongs to the user,
    // methods throw SteamUnavailableException when Steam can't be reached
    public interface ISteamSignInService
    {
        // the page of Steam where the user signs in, then Steam sends them to returnTo; realm is the site Steam names to the user
        public string GetSignInUrl(string returnTo, string realm);
        // the SteamID64 Steam vouches for, null when the answer isn't valid or was meant for another return address
        public Task<string> VerifyAsync(IReadOnlyDictionary<string, string> parameters, string returnTo, CancellationToken ct = default);
    }
}
