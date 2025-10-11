using Microsoft.EntityFrameworkCore;
using NetSuiteRAG.Api.Data;
using NetSuiteRAG.Api.Services.Implementations;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Extensions;
using NetSuiteRAG.Shared.Health;
using NetSuiteRAG.Shared.Middleware;
using NetSuiteRAG.Shared.Monitoring;
using Serilog;

// Configure Serilog early
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Replace default logging with Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext());

    // Add Aspire service integrations
    builder.AddNpgsqlDbContext<AppDbContext>("netsuitedb");
    builder.AddRedisClient("redis");

    // Add distributed caching using Redis
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("redis");
    });

    // Add OpenTelemetry
    builder.Services.AddNetSuiteRagTelemetry(builder.Configuration);

    // Register application services
    builder.Services.AddScoped<IFieldDictionaryService, FieldDictionaryService>();

    // Add health checks
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database");

    // Add metrics collector as singleton
    builder.Services.AddSingleton<MetricsCollector>();

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddOpenApi();

    var app = builder.Build();

    // Apply database migrations and seed data
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        try
        {
            logger.LogInformation("Applying database migrations...");
            await dbContext.Database.MigrateAsync();
            logger.LogInformation("Database migrations applied successfully");

            await FieldDefinitionSeeder.SeedAsync(dbContext, logger);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during database initialization");
            throw;
        }
    }

    // Add QueryId middleware first to ensure all logs have correlation
    app.UseMiddleware<QueryIdMiddleware>();

    // Add Serilog request logging
    app.UseSerilogRequestLogging(options =>
    {
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            if (httpContext.Items.TryGetValue("QueryId", out var queryId) && queryId != null)
            {
                diagnosticContext.Set("QueryId", queryId);
            }
        };
    });

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    // Only use HTTPS redirection when not running under Aspire
    if (!builder.Environment.IsDevelopment() || builder.Configuration["ASPNETCORE_URLS"] == null)
    {
        app.UseHttpsRedirection();
    }

    // Enable static files for dashboard
    app.UseStaticFiles();

    // Map controllers
    app.MapControllers();

    // Root endpoint for Aspire dashboard link
    app.MapGet("/", () => Results.Redirect("/dashboard.html"))
        .ExcludeFromDescription();

    // Health check endpoints
    app.MapHealthChecks("/health");
    app.MapHealthChecks("/health/ready");
    app.MapHealthChecks("/health/live");

    // Metrics endpoint
    app.MapGet("/api/metrics", (MetricsCollector metrics) =>
    {
        return Results.Ok(metrics.GetMetricsSummary());
    })
    .WithName("GetMetrics");

    // Sample endpoint for testing observability
    var summaries = new[]
    {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

    app.MapGet("/weatherforecast", (ILogger<Program> logger, MetricsCollector metrics) =>
    {
        var startTime = DateTime.UtcNow;
        logger.LogInformation("Generating weather forecast");

        metrics.IncrementCounter("weatherforecast.requests");

        var forecast = Enumerable.Range(1, 5).Select(index =>
            new WeatherForecast
            (
                DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                Random.Shared.Next(-20, 55),
                summaries[Random.Shared.Next(summaries.Length)]
            ))
            .ToArray();

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        metrics.RecordLatency("weatherforecast", elapsedMs);

        logger.LogInformation("Weather forecast generated with {Count} entries in {ElapsedMs}ms",
            forecast.Length, elapsedMs);

        return forecast;
    })
    .WithName("GetWeatherForecast");

    Log.Information("Starting NetSuiteRAG API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
