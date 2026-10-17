namespace Infrastructure.Configurations
{
    public class VideoGamesCriticOptions
    {
        public string Address { get; set; }

        // without a key the API is available anonymously, but limited to 60 requests an hour per server address
        public string ApiKey { get; set; }
    }
}
