using Application.DTO;
using Application.Enum;

namespace API.RepR.Response
{
    public class CheckGlobalEventPassingResponse
    {
        public PassingCheckResult PassingCheck { get; set; }
        public GlobalEventParticipationObject Participation { get; set; }
    }
}
