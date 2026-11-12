using Application.Consts;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    // the favorite games of the current user in their order, they replace the saved ones
    public class SetFavoriteGamesRequest
    {
        public List<string> GameIds { get; set; }
    }

    public class SetFavoriteGamesValidator : Validator<SetFavoriteGamesRequest>
    {
        public SetFavoriteGamesValidator()
        {
            RuleFor(r => r.GameIds).Cascade(CascadeMode.Stop).NotNull()
                .Must(ids => ids.Count <= PlayerLimits.MaxFavoriteGames).WithMessage($"A player can have up to {PlayerLimits.MaxFavoriteGames} favorite games")
                .Must(ids => ids.All(id => !string.IsNullOrWhiteSpace(id))).WithMessage("A game id can't be empty")
                .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("A game is added once");
        }
    }
}
