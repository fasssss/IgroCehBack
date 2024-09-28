using Persistence.Context;

namespace API.Configurations
{
    public static class PersistanceConfiguration
    {
        public static IServiceCollection AddPersistance(this IServiceCollection services)
        {
            services.AddDbContext<IgroCehContext>();
            return services;
        }
    }
}
