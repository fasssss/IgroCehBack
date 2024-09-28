using Infrastructure.ExternalInterfaces;
using Refit;

namespace API.Configurations
{
    public static class InfrastructureConfiguration
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddRefitClient<IDiscordApi>()
                .ConfigureHttpClient(config => config.BaseAddress = new Uri(configuration["DiscordApi:Address"] ?? ""));

            return services;
        }
    }
}
