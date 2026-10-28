using Application.Consts;
using Application.DTO;
using FastEndpoints;
using FluentValidation;

namespace API.RepR.Request
{
    // the image is uploaded with its own request after the event is created
    public class CreateGlobalEventRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        // the badge a player gets for passing the event, one of the uploaded badges
        public string BadgeId { get; set; }
        public List<NewGlobalEventGameObject> Games { get; set; }
    }

    public class CreateGlobalEventValidator : Validator<CreateGlobalEventRequest>
    {
        public CreateGlobalEventValidator()
        {
            RuleFor(r => r.Name).Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("The event needs a name")
                .MaximumLength(GlobalEventLimits.NameMaxLength);
            RuleFor(r => r.Description).MaximumLength(GlobalEventLimits.DescriptionMaxLength);
            RuleFor(r => r.BadgeId).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("The event needs a badge for its passing")
                .MaximumLength(BadgeLimits.IdMaxLength);
            RuleFor(r => r.Games).Cascade(CascadeMode.Stop).NotNull()
                .Must(games => games.Count >= GlobalEventLimits.MinGames).WithMessage($"A player rolls {GlobalEventLimits.MinGames} games, the event needs at least as many")
                .Must(games => games.Count <= GlobalEventLimits.MaxGames).WithMessage($"An event can have up to {GlobalEventLimits.MaxGames} games")
                .Must(games => games.Select(g => g.AppId).Distinct().Count() == games.Count).WithMessage("A game is added once");
            RuleForEach(r => r.Games).ChildRules(game =>
            {
                // a game without achievements is passed only by a claim the administrator checks by hand
                game.RuleFor(g => g.AchievementApiNames).Cascade(CascadeMode.Stop).NotNull()
                    .Must(names => names.Count <= GlobalEventLimits.MaxAchievementsPerGame).WithMessage($"A game can have up to {GlobalEventLimits.MaxAchievementsPerGame} achievements")
                    .Must(names => names.Distinct().Count() == names.Count).WithMessage("An achievement is added once");
            });
        }
    }
}
