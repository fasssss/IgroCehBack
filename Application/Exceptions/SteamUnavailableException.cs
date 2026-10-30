namespace Application.Exceptions
{
    // Steam can't be reached, refuses the key or there is no key
    public class SteamUnavailableException : AchievementPlatformUnavailableException
    {
        public SteamUnavailableException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
