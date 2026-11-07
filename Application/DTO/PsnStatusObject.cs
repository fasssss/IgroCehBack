namespace Application.DTO
{
    // how the service account of the site stands in PSN, for the administrator; it holds no tokens
    public class PsnStatusObject
    {
        // a request of the site would get a token now
        public bool IsSignInWorking { get; set; }
        // PSN refused the NPSSO or the settings have none, only a new one helps; otherwise a sign-in that doesn't work is PSN being down or the limit of the site
        public bool IsNpssoRefused { get; set; }
        // the day typed in PsnApiOptions.NpssoExpiresAt, null when the settings have none or it can't be read
        public DateTimeOffset? NpssoExpiresAt { get; set; }
        // NpssoExpiresAt has passed or is less than PsnLimits.NpssoWarningDays away; the day may be old, so it says nothing about the sign-in itself
        public bool IsNpssoEnding { get; set; }
        // when the site has to sign in by the NPSSO again, null before the first sign-in
        public DateTimeOffset? RefreshTokenExpiresAt { get; set; }
    }
}
