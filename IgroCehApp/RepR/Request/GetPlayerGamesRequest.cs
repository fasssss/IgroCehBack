using Application.Enum;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    public class GetPlayerGamesRequest
    {
        public string UserId { get; set; }
        public PlayerGamesKind? Kind { get; set; }
        // the latest first when empty
        public PlayerGamesSort? Sort { get; set; }
        public int StartFrom { get; set; }
    }

    public class GetPlayerGamesValidator : Validator<GetPlayerGamesRequest>
    {
        public GetPlayerGamesValidator()
        {
            RuleFor(r => r.Kind).NotNull().IsInEnum();
            RuleFor(r => r.Sort).IsInEnum();
        }
    }
}
