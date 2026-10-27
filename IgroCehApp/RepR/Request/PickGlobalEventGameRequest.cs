namespace API.RepR.Request
{
    public class PickGlobalEventGameRequest
    {
        public string GlobalEventId { get; set; }
        // the Steam app id of one of the rolled games
        public int AppId { get; set; }
    }
}
