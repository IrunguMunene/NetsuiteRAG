using Microsoft.EntityFrameworkCore;
using NetSuiteRAG.Api.Data;
using NetSuiteRAG.Api.Data.Seeds;
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
    builder.Services.AddScoped<INetSuiteApiService, NetSuiteApiService>();
    builder.Services.AddScoped<ICustomFieldCrawlerService, CustomFieldCrawlerService>();
    builder.Services.AddScoped<IOllamaEmbeddingService, OllamaEmbeddingService>();
    builder.Services.AddScoped<IFieldEnrichmentService, FieldEnrichmentService>();
    builder.Services.AddScoped<IGlossaryService, GlossaryService>();
    builder.Services.AddSingleton<IVectorStoreService, QdrantVectorService>();
    builder.Services.AddScoped<IIndexingService, IndexingService>();

    // Register background services
    builder.Services.AddHostedService<CustomFieldCrawlerBackgroundService>();
    builder.Services.AddHostedService<FieldEnrichmentBackgroundService>();

    // Add HttpClientFactory for NetSuite API calls
    builder.Services.AddHttpClient();

    // Add health checks
    builder.Services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database");

    // Add metrics collector as singleton
    builder.Services.AddSingleton<MetricsCollector>();

    // Add services to the container
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new()
        {
            Title = "NetSuite RAG API",
            Version = "v1",
            Description = "RAG-based NetSuite reporting API with natural language query planning",
            Contact = new()
            {
                Name = "NetSuite RAG Team"
            }
        });

        // Include XML comments for better documentation
        var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
        {
            options.IncludeXmlComments(xmlPath);
        }
    });

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
            await GlossarySeeder.SeedAsync(dbContext, logger);
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
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "NetSuite RAG API v1");
            options.RoutePrefix = "swagger";
            options.DocumentTitle = "NetSuite RAG API Documentation";
        });
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
