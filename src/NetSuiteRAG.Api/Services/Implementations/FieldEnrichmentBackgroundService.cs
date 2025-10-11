using NetSuiteRAG.Api.Services.Interfaces;

namespace NetSuiteRAG.Api.Services.Implementations;

/// <summary>
/// Background service that periodically enriches field definitions.
/// </summary>
public class FieldEnrichmentBackgroundService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<FieldEnrichmentBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan _enrichmentInterval = TimeSpan.FromHours(
        configuration.GetValue<int>("FieldEnrichment:IntervalHours", 24));

    /// <summary>
    /// Executes the background enrichment task.
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Field Enrichment Background Service started. Interval: {Interval} hours",
            _enrichmentInterval.TotalHours);

        // Run enrichment immediately on startup if configured
        var runOnStartup = configuration.GetValue<bool>("FieldEnrichment:RunOnStartup", false);
        if (runOnStartup)
        {
            await RunEnrichmentAsync(stoppingToken);
        }

        using var timer = new PeriodicTimer(_enrichmentInterval);

        try
        {
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunEnrichmentAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Field Enrichment Background Service is stopping");
        }
    }

    /// <summary>
    /// Runs the enrichment process.
    /// </summary>
    private async Task RunEnrichmentAsync(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        logger.LogInformation("Starting scheduled field enrichment at {StartTime}", startTime);

        try
        {
            // Create a new scope for scoped services
            using var scope = serviceProvider.CreateScope();
            var enrichmentService = scope.ServiceProvider
                .GetRequiredService<IFieldEnrichmentService>();

            var result = await enrichmentService.EnrichAllFieldsAsync(cancellationToken);

            if (result.IsSuccess)
            {
                var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
                logger.LogInformation(
                    "Field enrichment completed successfully in {ElapsedMs}ms. " +
                    "Processed: {Total}, Success: {Success}, Failed: {Failed}, Skipped: {Skipped}",
                    elapsedMs,
                    result.Value?.TotalProcessed,
                    result.Value?.SuccessfullyEnriched,
                    result.Value?.Failed,
                    result.Value?.Skipped);
            }
            else
            {
                logger.LogError("Field enrichment failed: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error during scheduled field enrichment");
        }
    }
}
