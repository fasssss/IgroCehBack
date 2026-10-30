namespace Application.Exceptions
{
    // a platform whose achievements prove the passing of a game (Steam, Xbox) doesn't answer
    public abstract class AchievementPlatformUnavailableException : Exception
    {
        protected AchievementPlatformUnavailableException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
