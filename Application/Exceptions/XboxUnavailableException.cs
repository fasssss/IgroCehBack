namespace Application.Exceptions
{
    // OpenXBL, which stands between the site and Xbox Live, can't be reached or fails
    public class XboxUnavailableException : AchievementPlatformUnavailableException
    {
        public XboxUnavailableException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
