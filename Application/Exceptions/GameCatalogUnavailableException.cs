namespace Application.Exceptions
{
    // catalog is down, its quota is exhausted or it answered with an unexpected error
    public class GameCatalogUnavailableException : Exception
    {
        public GameCatalogUnavailableException(string message, Exception innerException = null)
            : base(message, innerException)
        {
        }
    }
}
