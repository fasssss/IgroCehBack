using System.Text.RegularExpressions;

namespace Application.Consts
{
    // the database columns, the validation and the requests to PSN share these limits and the format of a game in PSN
    public static class PsnLimits
    {
        // an account id is a 64-bit number, e.g. 2984038888603282554
        public const int AccountIdMaxLength = 20;
        // an online id is 3 to 16 letters, digits, hyphens and underscores
        public const int OnlineIdMaxLength = 16;
        public const int CommunicationIdMaxLength = 20;
        // the trophy lists of PS5 games live in trophy2, the ones of PS3, PS4 and Vita games in trophy; the same id in the other one answers 404
        public const string Ps5TrophyService = "trophy2";
        public const string OlderTrophyService = "trophy";

        private static readonly Regex TitleRefFormat = new(@"^(NPWR\d{5,}_\d{2})(?:/(trophy2?))?$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex OnlineIdFormat = new(@"^[A-Za-z0-9_-]{3,16}$", RegexOptions.CultureInvariant);

        // a game in PSN is the id of its trophy list and the service that keeps it, NPWR20188_00/trophy2; the administrator types the id alone,
        // PSN shows which service keeps it, and the site saves the ref with both; the service is empty for the id alone
        public static bool TryParseTitleRef(string titleRef, out string communicationId, out string service)
        {
            var match = TitleRefFormat.Match(titleRef ?? "");
            communicationId = match.Success ? match.Groups[1].Value.ToUpperInvariant() : null;
            service = match.Success && match.Groups[2].Success ? match.Groups[2].Value.ToLowerInvariant() : null;
            return match.Success && communicationId.Length <= CommunicationIdMaxLength;
        }

        public static string FormatTitleRef(string communicationId, string service) => service == null ? communicationId : $"{communicationId}/{service}";

        public static bool IsOnlineId(string onlineId) => OnlineIdFormat.IsMatch(onlineId ?? "");

        public static bool IsAccountId(string accountId) => !string.IsNullOrEmpty(accountId) && accountId.Length <= AccountIdMaxLength && accountId.All(char.IsAsciiDigit);
    }
}
