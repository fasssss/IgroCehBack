namespace API.RepR.Request
{
    public class GetCommunityReviewsRequest
    {
        // how many of the newest reviews to return, the page asks for more as it is scrolled
        public int Count { get; set; }
    }
}
