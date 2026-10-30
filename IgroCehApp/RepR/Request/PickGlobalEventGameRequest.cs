namespace API.RepR.Request
{
    public class PickGlobalEventGameRequest
    {
        public string GlobalEventId { get; set; }
        // the catalog id of one of the rolled games (VgcId)
        public int AppId { get; set; }
    }
}
