using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Controllers;

public static class Startup
{
    public static IServiceCollection AddEndpointControllers(this IServiceCollection services)
    {

        services.AddControllers();
        services.AddEndpointsApiExplorer();

        return services;
    }
}
