using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using WorkTracker.Common;
using WorkTracker.Common.Configuration;
using WorkTracker.Common.Extensions;
using WorkTracker.Common.Interfaces;
using WorkTracker.Common.Repositories;
using WorkTracker.Common.Services;

namespace Microsoft.Extensions.Hosting;

// Adds common Aspire services: service discovery, resilience, health checks, and OpenTelemetry.
// This project should be referenced by each service project in your solution.
// To learn more about using this project, see https://aka.ms/dotnet/aspire/service-defaults
public static class Extensions
{
    private const string HealthEndpointPath = "/health";
    private const string AlivenessEndpointPath = "/alive";

    public static TBuilder AddServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var configuration = builder.Configuration;

        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        return builder;
    }

    public static TBuilder AddApiServiceDefaults<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var configuration = builder.Configuration;

        builder.ConfigureOpenTelemetry();
        builder.AddDefaultHealthChecks();
        builder.Services.AddServiceDiscovery();

        builder.Services.ConfigureHttpClientDefaults(http =>
        {
            http.AddStandardResilienceHandler();
            http.AddServiceDiscovery();
        });

        // Common
        builder.Services.AddProblemDetails();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
        builder.AddRedisClient(connectionName: "worktracker-cache");
        builder.AddRabbitMQClient(connectionName: "worktracker-queue");

        // Services & Repositories
        builder.Services.AddSingleton<IEnvironmentConfiguration, EnvironmentConfiguration>();
        builder.Services.AddScoped<ICacheService, CacheService>()
            .AddScoped<IAuthService, AuthService>()
            .AddScoped<IUserRepository, UserRepository>()
            .AddSingleton<IEventPublisher, RabbitMqEventPublisher>()
            .AddHostedService<RabbitMqTopologyService>();

        builder.Services.AddSingleton<BackgroundEventPublisherService>();
        builder.Services.AddSingleton<IBackgroundEventPublisher>(serviceProvider => serviceProvider.GetRequiredService<BackgroundEventPublisherService>());
        builder.Services.AddHostedService(serviceProvider => serviceProvider.GetRequiredService<BackgroundEventPublisherService>());

        // Database
        builder.Services.AddDatabaseConfiguration(configuration);

        // Options
        builder.Services.AddOptions(configuration);

        // Auth
        builder.Services.AddWorkTrackerAuthentication(configuration);

        return builder;
    }

    public static TBuilder ConfigureOpenTelemetry<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation();
            })
            .WithTracing(tracing =>
            {
                tracing.AddSource(builder.Environment.ApplicationName)
                    .AddAspNetCoreInstrumentation(tracing =>
                        tracing.Filter = context =>
                            !context.Request.Path.StartsWithSegments(HealthEndpointPath)
                            && !context.Request.Path.StartsWithSegments(AlivenessEndpointPath)
                    )
                    .AddHttpClientInstrumentation();
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static TBuilder AddOpenTelemetryExporters<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]);

        if (useOtlpExporter)
        {
            builder.Services.AddOpenTelemetry().UseOtlpExporter();
        }

        return builder;
    }

    public static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtConfiguration>(configuration.GetSection(JwtConfiguration.Section));

        return services;
    }

    public static IServiceCollection AddDatabaseConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        var workTrackerConnectionString = configuration.GetConnectionString("worktracker")!;

        services.AddDbContext<WorkTrackerDbContext>(options =>
            options.UseNpgsql(workTrackerConnectionString)
        );

        services.AddHealthChecks()
            .AddNpgSql(connectionString: workTrackerConnectionString);

        return services;
    }

    public static IServiceCollection AddWorkTrackerAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = configuration
                    .GetSection(JwtConfiguration.Section)
                    .Get<JwtConfiguration>()!;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidIssuer = jwtOptions.Issuer,
                    RequireExpirationTime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                    ValidateIssuerSigningKey = true,
                };
            });

        var requireAuthPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();

        services.AddAuthorizationBuilder()
            .SetDefaultPolicy(requireAuthPolicy);

        return services;
    }

    public static TBuilder AddDefaultHealthChecks<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), ["live"]);

        return builder;
    }

    public static WebApplication MapDefaultEndpoints(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapHealthChecks(HealthEndpointPath);
            app.MapHealthChecks(AlivenessEndpointPath, new HealthCheckOptions
            {
                Predicate = r => r.Tags.Contains("live")
            });
        }

        return app;
    }

    public static async Task<WebApplication> UseTestMigrationAsync<TBuilder>(this WebApplication app, TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        if (builder.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();

            await scope.ServiceProvider.GetRequiredService<WorkTrackerDbContext>()
                .RunTestMigrationAsync();
        }

        return app;
    }
}
