using Infrastructure.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class Startup
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {
        builder.AddServiceDefaults();
        builder.Services.AddDatabase(builder.Configuration);

        return builder;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.UseServiceDefaults();

        return app;
    }
}
