namespace API.RepR.Request
{
    public class CreateGameRequest
    {
        public string GameName { get; set; }
        public IFormFile Image { get; set; }
    }
}
