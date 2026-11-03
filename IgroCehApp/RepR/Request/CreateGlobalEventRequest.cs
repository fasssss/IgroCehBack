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
                // a game without achievements is passed only by a claim the administrator checks by hand; without the platforms
                // a tab opened before them would clear the achievements of the game, so a game comes with one of the contracts
                game.RuleFor(g => g.Platforms).Cascade(CascadeMode.Stop)
                    .NotNull().WithMessage("A game needs its platforms").When(g => g.AchievementApiNames == null);
                game.RuleFor(g => g.Platforms)
                    .Must(platforms => platforms.Select(p => p.Platform).Distinct().Count() == platforms.Count).WithMessage("A platform is added once")
                    .When(g => g.Platforms != null);
                game.RuleForEach(g => g.Platforms).ChildRules(platform =>
                {
                    platform.RuleFor(p => p.Platform).IsInEnum();
                    // the format of the game on the platform is the platform's own, a wrong one is an unknown achievement
                    platform.RuleFor(p => p.TitleRef).MaximumLength(GlobalEventLimits.TitleRefMaxLength);
                    platform.RuleFor(p => p.AchievementIds).Cascade(CascadeMode.Stop).NotNull()
                        .Must(ids => ids.Count <= GlobalEventLimits.MaxAchievementsPerGame).WithMessage($"A game can have up to {GlobalEventLimits.MaxAchievementsPerGame} achievements on a platform")
                        .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("An achievement is added once");
                }).When(g => g.Platforms != null);

                // the old contract, removed once every front sends the platforms
                game.RuleFor(g => g.AchievementApiNames)
                    .Must(names => names.Count <= GlobalEventLimits.MaxAchievementsPerGame).WithMessage($"A game can have up to {GlobalEventLimits.MaxAchievementsPerGame} achievements")
                    .Must(names => names.Distinct().Count() == names.Count).WithMessage("An achievement is added once")
                    .When(g => g.Platforms == null && g.AchievementApiNames != null);
                // the same limit for the achievements in Xbox, they need their game in Xbox
                game.RuleFor(g => g.XboxAchievementIds)
                    .Must(ids => ids.Count <= GlobalEventLimits.MaxAchievementsPerGame).WithMessage($"A game can have up to {GlobalEventLimits.MaxAchievementsPerGame} achievements in Xbox")
                    .Must(ids => ids.Distinct().Count() == ids.Count).WithMessage("An achievement is added once")
                    .When(g => g.Platforms == null && g.XboxAchievementIds != null);
                game.RuleFor(g => g.XboxTitleId).Cascade(CascadeMode.Stop)
                    .NotEmpty().WithMessage("The achievements in Xbox need their game in Xbox")
                    .Matches(@"^\d+$").MaximumLength(XboxLimits.TitleIdMaxLength)
                    .When(g => g.Platforms == null && g.XboxAchievementIds?.Count > 0);
            });
        }
    }
}
