using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Shared.EntityFramework.Interceptors;

namespace Shared.EntityFramework
{
    public static class Startup
    {
        public static IServiceCollection AddEntityFramework<T>(this IServiceCollection services, string connectionString)
                where T : DbContext
        {
            services.AddDbContext<T>(options =>
            {
                options.UseSqlServer(connectionString);
                options.AddInterceptors(services.BuildServiceProvider().GetRequiredService<AuditSaveChangesInterceptor>());
            });

            services.AddScoped<AuditSaveChangesInterceptor>();

            return services;
        }

        public static Task RunMigrations<T>(this IServiceProvider serviceProvider)
            where T : DbContext
        {
            using var scope = serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<T>();
            return dbContext.Database.MigrateAsync();
        }
    }
}
