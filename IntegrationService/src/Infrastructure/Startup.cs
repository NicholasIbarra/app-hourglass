using Infrastructure.Controllers;
using Infrastructure.OpenApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;

namespace Infrastructure;

public static class Startup
{
    public static IHostApplicationBuilder AddInfrastructure(this IHostApplicationBuilder builder)
    {

        builder.AddServiceDefaults();
        builder.Services.AddEndpointControllers();
        builder.Services.AddSwagger();

        return builder;
    }

    public static WebApplication UseInfrastructure(this WebApplication app)
    {
        app.MapDefaultEndpoints();
        app.MapControllers();
        app.UseHttpsRedirection();
        app.UseOpenApi();

        return app;
    }
}
