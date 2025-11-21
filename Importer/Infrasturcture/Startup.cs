using Application;
using Hangfire;
using Infrasturcture.Persistence;
using Infrasturcture.Storage;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Infrasturcture
{
    public static class Startup
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Infrastructure service registrations go here

            services.AddPersistence(configuration);
            services.AddSignalR();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowClient", builder =>
                {
                    builder.WithOrigins("http://localhost:5173")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowCredentials(); // REQUIRED for SignalR
                });
            });


            services.AddSingleton<IBlobStorage>(sp =>
            {
                var logger = sp.GetService<ILogger<LocalBlobStorage>>();
                return new LocalBlobStorage("c:\\blob-storage", logger);
            });

            services.AddHangfire(config =>
                config
                    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                    .UseColouredConsoleLogProvider()
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseRecommendedSerializerSettings()
                    .UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"))
            );

            services.AddHangfireServer();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = "https://nibar.us.auth0.com/";
                options.Audience = "hourglass-importer";
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = ClaimTypes.NameIdentifier
                };
            });


            return services;
        }

        public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddScoped<DbContext, ApplicationDbContext>()
                .AddScoped<IApplicationDbContext, ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
            );


            return services;
        }

        public static WebApplication UseInfrastructure(this WebApplication app)
        {
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors("AllowClient");

            app.UseHangfireDashboard();

            app.MapHub<ImportsHub>("/hubs/imports");


            return app;
        }
    }
}
