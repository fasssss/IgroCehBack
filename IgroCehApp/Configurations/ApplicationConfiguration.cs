using Application.ApplicationInterfaces;
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
            return services;
        }
    }
}
