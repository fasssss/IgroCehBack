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
            return services;
        }
    }
}
