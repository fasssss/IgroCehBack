using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace API.Configurations
{
    public static class MigrationExtensions
    {
        public static async Task ApplyMigrationsAsync(this IHost app)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<IgroCehContext>();
            await db.Database.MigrateAsync();
        }
    }
}
