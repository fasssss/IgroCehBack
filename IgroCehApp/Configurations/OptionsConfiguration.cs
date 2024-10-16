using Infrastructure.Configurations;

namespace API.Configurations
{
    public static class OptionsConfiguration
    {
        public static IServiceCollection AddOptionsConfiguration(this IServiceCollection services, IConfiguration config) 
        {
            services.Configure<DiscordApiOptions>(config.GetSection(nameof(DiscordApiOptions)));
            return services;
        }
    }
}
