using Application.Interfaces;
using Infrastructure.Configurations;
using Infrastructure.ExternalInterfaces;
using Infrastructure.Services;
using Microsoft.Extensions.Options;
using Refit;
using System.Net.Http.Headers;

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

            services.AddRefitClient<IVideoGamesCriticApi>()
                .ConfigureHttpClient((serviceProvider, config) =>
                {
                    var videoGamesCriticOptions = serviceProvider.GetRequiredService<IOptions<VideoGamesCriticOptions>>().Value;
                    config.BaseAddress = new Uri(videoGamesCriticOptions.Address ?? "");
                    // empty key means anonymous access
                    if (!string.IsNullOrWhiteSpace(videoGamesCriticOptions.ApiKey))
                    {
                        config.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", videoGamesCriticOptions.ApiKey);
                    }
                });
            services.AddMemoryCache();
            services.AddTransient<IGameCatalogService, VideoGamesCriticCatalogService>();
            return services;
        }
    }
}
