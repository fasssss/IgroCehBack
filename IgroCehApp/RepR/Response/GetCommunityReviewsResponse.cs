using Application.DTO;

namespace API.RepR.Response
{
    public class GetCommunityReviewsResponse
    {
        public List<CommunityReviewObject> Reviews { get; set; }
        public bool HasMore { get; set; }
    }
}
