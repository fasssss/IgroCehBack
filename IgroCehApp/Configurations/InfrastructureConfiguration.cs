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
            services.AddRefitClient<ISteamWebApi>()
                .ConfigureHttpClient((serviceProvider, config) =>
                    config.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<SteamApiOptions>>().Value.Address ?? ""));
            services.AddRefitClient<ISteamCommunityApi>()
                .ConfigureHttpClient((serviceProvider, config) =>
                    config.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<SteamApiOptions>>().Value.CommunityAddress ?? ""));
            services.AddRefitClient<IOpenXblApi>()
                .ConfigureHttpClient((serviceProvider, config) =>
                    config.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<XboxApiOptions>>().Value.Address ?? ""));
            services.AddRefitClient<IPsnAuthApi>()
                .ConfigureHttpClient((serviceProvider, config) =>
                    config.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<PsnApiOptions>>().Value.AuthAddress ?? ""))
                // the code of the sign-in is in the address of a redirect to the app, and the npsso cookie is set on the request by hand
                .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler { AllowAutoRedirect = false, UseCookies = false })
                // HttpClient logs the headers at Trace, these carry the NPSSO, the tokens and the code
                .RedactLoggedHeaders(_ => true);
            services.AddRefitClient<IPsnApi>()
                .ConfigureHttpClient((serviceProvider, config) =>
                    config.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<PsnApiOptions>>().Value.ApiAddress ?? ""))
                .RedactLoggedHeaders(_ => true);
            services.AddRefitClient<IPsnProfileApi>()
                .ConfigureHttpClient((serviceProvider, config) =>
                    config.BaseAddress = new Uri(serviceProvider.GetRequiredService<IOptions<PsnApiOptions>>().Value.ProfileAddress ?? ""))
                .RedactLoggedHeaders(_ => true);
            services.AddMemoryCache();
            services.AddTransient<IGameCatalogService, VideoGamesCriticCatalogService>();
            services.AddTransient<ISteamService, SteamService>();
            services.AddTransient<ISteamSignInService, SteamSignInService>();
            services.AddTransient<IXboxSignInService, XboxSignInService>();
            services.AddTransient<IXboxService, XboxService>();
            services.AddSingleton<PsnRateLimiter>();
            services.AddSingleton<PsnTokenStore>();
            services.AddTransient<IPsnService, PsnService>();
            services.AddSingleton<IPsnLinkStore, PsnLinkStore>();
            return services;
        }
    }
}
