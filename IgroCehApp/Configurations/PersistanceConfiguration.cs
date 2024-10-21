﻿using Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;
using Persistence.Repository;

namespace API.Configurations
{
    public static class PersistanceConfiguration
    {
        public static IServiceCollection AddPersistance(this IServiceCollection services, IConfiguration configuration)
        {
            var defaultConnectionString = configuration.GetConnectionString("Default");
            var mySqlVersion = configuration.GetValue<string>("MySQLVersion") ?? "8.0.27";
            services.AddMySql<IgroCehContext>(defaultConnectionString, new MySqlServerVersion(new Version(mySqlVersion)));
            services.AddScoped<IUserRepository, UserRepository>();
            return services;
        }
    }
}
