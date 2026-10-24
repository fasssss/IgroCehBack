using Application.DTO;

namespace API.RepR.Response
{
    public class GetPlayerPreferencesResponse
    {
        public List<PreferenceTagObject> Preferred { get; set; }
        public List<PreferenceTagObject> Avoided { get; set; }
    }
}
