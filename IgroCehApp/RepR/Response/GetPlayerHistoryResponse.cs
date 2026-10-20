using Application.DTO;

namespace API.RepR.Response
{
    public class GetPlayerHistoryResponse
    {
        public List<PlayerHistoryItemObject> HistoryItems { get; set; }
        public bool HasMore { get; set; }
    }
}
