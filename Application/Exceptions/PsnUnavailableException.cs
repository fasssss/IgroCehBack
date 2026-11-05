namespace Application.Exceptions
{
    // PSN can't be reached or fails, it refuses the sign-in of the service account, or the requests of the site ran over their limit
    public class PsnUnavailableException : AchievementPlatformUnavailableException
    {
        public PsnUnavailableException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
