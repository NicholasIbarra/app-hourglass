using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
            });

            return services;
        }

    }
}
