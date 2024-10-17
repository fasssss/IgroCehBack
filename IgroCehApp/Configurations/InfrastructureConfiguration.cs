using Application.ApplicationInterfaces;
using Application.Interfaces;
using Application.Services;
using Infrastructure.Configurations;
using Infrastructure.ExternalInterfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Options;
using Refit;

namespace API.Configurations
{
    public static class InfrastructureConfiguration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            DiscordApiOptions discordApiOptions = services.BuildServiceProvider().GetRequiredService<IOptions<DiscordApiOptions>>().Value;
            services.AddRefitClient<IDiscordApi>()
                .ConfigureHttpClient(config => config.BaseAddress = new Uri(discordApiOptions.Address ?? ""));

            services.AddTransient<IAuthorizationService, DiscordAuthorizationService>();
            services.AddTransient<IAuthorizationApplicationService, AuthorizationApplicationService>();
            return services;
        }
    }
}
