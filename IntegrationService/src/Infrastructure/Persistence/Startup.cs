using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Persistence;

public static class Startup
{
    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        return services;
    }
}
