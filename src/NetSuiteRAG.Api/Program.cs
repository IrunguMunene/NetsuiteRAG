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
    builder.AddNpgsqlDataSource("netsuitedb");
    builder.AddRedisClient("redis");

    // Add OpenTelemetry
    builder.Services.AddNetSuiteRagTelemetry(builder.Configuration);

    // Add health checks
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database");

    // Add metrics collector as singleton
    builder.Services.AddSingleton<MetricsCollector>();

    // Add services to the container
    builder.Services.AddOpenApi();

    var app = builder.Build();

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
