using Application.DTO;

namespace API.RepR.Response
{
    public class GetReviewSuggestionsResponse
    {
        public List<ReviewSuggestionObject> ToWrite { get; set; }
        public List<ReviewSuggestionObject> ToAnswer { get; set; }
    }
}
