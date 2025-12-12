using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace ServiceDefaults.Mediator;

public static class Startup
{
    public static IServiceCollection AddMediatrBehaviors(this IServiceCollection services)
    {
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}