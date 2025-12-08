using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace Infrastructure.OpenApi;

public static class Startup
{
    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddSwaggerGen(options => 
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "API",
                Description = "An ASP.NET Core Web API",
                TermsOfService = new Uri("https://example.com/terms"),
                Contact = new OpenApiContact
                {
                    Name = "Example Contact",
                    Url = new Uri("https://example.com/contact")
                },
                License = new OpenApiLicense
                {
                    Name = "Example License",
                    Url = new Uri("https://example.com/license")
                }
            });
        });

        return services;
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
