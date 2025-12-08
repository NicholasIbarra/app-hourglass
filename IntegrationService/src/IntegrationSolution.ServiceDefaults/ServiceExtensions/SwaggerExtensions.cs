using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntegrationSolution.ServiceDefaults.DefaultExtensions;

public static class SwaggerExtensions
{
    public static TBuilder AddSwagger<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen();

        return builder;
    }

    public static WebApplication UseOpenApi(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "API");
                options.RoutePrefix = "swagger";
            });
        }

        return app;
    }
}