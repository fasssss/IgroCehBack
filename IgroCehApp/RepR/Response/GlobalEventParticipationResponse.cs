using Application.DTO;

namespace API.RepR.Response
{
    public class GlobalEventParticipationResponse
    {
        // null when the player hasn't rolled the games of the event yet
        public GlobalEventParticipationObject Participation { get; set; }
    }
}
