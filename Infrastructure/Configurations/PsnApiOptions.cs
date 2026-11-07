namespace Infrastructure.Configurations
{
    // the API of the PlayStation App, Sony doesn't publish it (https://psn-api.achievements.app describes it); the site uses it as a service account of its own
    public class PsnApiOptions
    {
        public string AuthAddress { get; set; }
        public string ApiAddress { get; set; }
        public string ProfileAddress { get; set; }

        // the public client of the PlayStation App, the same psn-api and PSNAWP use; without the NPSSO they open nothing, so they aren't secrets
        public string ClientId { get; set; }
        public string ClientAuthorization { get; set; }
        public string RedirectUri { get; set; }
        public string Scope { get; set; }

        // the npsso cookie of the service account, a secret: it opens the whole account; from appsettings, user secrets or the environment: PsnApiOptions__Npsso.
        // Empty means PSN is unavailable
        public string Npsso { get; set; }

        // the day the NPSSO ends, yyyy-MM-dd, optional: whoever renews it counts the day from expires_in of the answer that gives the NPSSO.
        // Only the warning about the end depends on it, a day that is missing, old or wrong breaks nothing;
        // a text and not a date, because a date that can't be read fails every class that takes these options
        public string NpssoExpiresAt { get; set; }

        // Sony publishes no limit, PSNAWP keeps to about 300 requests in 15 minutes and warns that more can ban the account
        public int RequestsPerWindow { get; set; } = 250;
        public int WindowMinutes { get; set; } = 15;

        // a refused sign-in isn't repeated for this long, a series of them would look like an attack on the account
        public int SignInRetrySeconds { get; set; } = 300;
    }
}
