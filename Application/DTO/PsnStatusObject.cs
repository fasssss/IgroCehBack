namespace Application.DTO
{
    // how the service account of the site stands in PSN, for the administrator; it holds no tokens
    public class PsnStatusObject
    {
        // a request of the site would get a token now
        public bool IsSignInWorking { get; set; }
        // PSN refused the NPSSO or the settings have none, only a new one helps; the live tokens may keep the sign-in working for up to 10 days more.
        // Otherwise a sign-in that doesn't work is PSN being down or the limit of the site
        public bool IsNpssoRefused { get; set; }
        // as PSN told it, null while it hasn't told
        public DateTimeOffset? NpssoExpiresAt { get; set; }
        // NpssoExpiresAt is less than PsnLimits.NpssoWarningDays away
        public bool IsNpssoEnding { get; set; }
        // when the site has to sign in by the NPSSO again, null before the first sign-in
        public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
    }
}
