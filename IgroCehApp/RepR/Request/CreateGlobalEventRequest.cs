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
        public List<NewGlobalEventGameObject> Games { get; set; }
    }

    public class CreateGlobalEventValidator : Validator<CreateGlobalEventRequest>
    {
        public CreateGlobalEventValidator()
        {
            RuleFor(r => r.Name).Must(name => !string.IsNullOrWhiteSpace(name)).WithMessage("The event needs a name")
                .MaximumLength(GlobalEventLimits.NameMaxLength);
            RuleFor(r => r.Description).MaximumLength(GlobalEventLimits.DescriptionMaxLength);
            RuleFor(r => r.Games).Cascade(CascadeMode.Stop).NotNull()
                .Must(games => games.Count >= GlobalEventLimits.MinGames).WithMessage($"A player rolls {GlobalEventLimits.MinGames} games, the event needs at least as many")
                .Must(games => games.Count <= GlobalEventLimits.MaxGames).WithMessage($"An event can have up to {GlobalEventLimits.MaxGames} games")
                .Must(games => games.Select(g => g.AppId).Distinct().Count() == games.Count).WithMessage("A game is added once");
            RuleForEach(r => r.Games).ChildRules(game =>
            {
                game.RuleFor(g => g.AchievementApiNames).Cascade(CascadeMode.Stop).NotEmpty().WithMessage("Every game needs an achievement that means it is passed")
                    .Must(names => names.Count <= GlobalEventLimits.MaxAchievementsPerGame).WithMessage($"A game can have up to {GlobalEventLimits.MaxAchievementsPerGame} achievements")
                    .Must(names => names.Distinct().Count() == names.Count).WithMessage("An achievement is added once");
            });
        }
    }
}
