using Application.ApplicationInterfaces;
using Application.Interfaces;
using Application.Services;

namespace API.Configurations
{
    public static class ApplicationConfiguration
    {
        public static IServiceCollection AddApplicationConfiguration(this IServiceCollection services) 
        {
            services.AddTransient<IAuthorizationApplicationService, AuthorizationApplicationService>();
            services.AddTransient<IGuildApplicationService, GuildApplicationService>();
            services.AddTransient<IEventApplicationService, EventApplicationService>();
            services.AddTransient<IGameApplicationService, GameApplicationService>();
            services.AddTransient<IPlayerApplicationService, PlayerApplicationService>();
            services.AddTransient<IReviewApplicationService, ReviewApplicationService>();
            services.AddTransient<INewcomerPoolApplicationService, NewcomerPoolApplicationService>();
            services.AddTransient<IGlobalEventApplicationService, GlobalEventApplicationService>();
            services.AddTransient<IGlobalEventParticipationApplicationService, GlobalEventParticipationApplicationService>();
            // the platforms whose achievements prove the passing of a game of a global event, one per AchievementPlatform
            services.AddTransient<IAchievementPlatformChecker, SteamAchievementChecker>();
            services.AddTransient<IAchievementPlatformChecker, XboxAchievementChecker>();
            // the platforms the administrator chooses the achievements of a game on, one per AchievementPlatform next to its checker
            services.AddTransient<IAchievementPlatformCatalog, SteamAchievementCatalog>();
            services.AddTransient<IAchievementPlatformCatalog, XboxAchievementCatalog>();
            services.AddTransient<IAchievementPlatformCatalog, PsnAchievementCatalog>();
            services.AddTransient<IGlobalEventClaimApplicationService, GlobalEventClaimApplicationService>();
            services.AddTransient<IBadgeApplicationService, BadgeApplicationService>();
            services.AddTransient<ISteamLinkApplicationService, SteamLinkApplicationService>();
            services.AddTransient<IXboxLinkApplicationService, XboxLinkApplicationService>();
            services.AddTransient<IPsnLinkApplicationService, PsnLinkApplicationService>();
            return services;
        }
    }
}
