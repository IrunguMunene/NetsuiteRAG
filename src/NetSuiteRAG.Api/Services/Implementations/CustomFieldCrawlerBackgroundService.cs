using NetSuiteRAG.Api.Services.Interfaces;

namespace NetSuiteRAG.Api.Services.Implementations;

/// <summary>
/// Background service that runs the custom field crawler on a daily schedule.
/// </summary>
public class CustomFieldCrawlerBackgroundService(
    IServiceProvider serviceProvider,
    IConfiguration configuration,
    ILogger<CustomFieldCrawlerBackgroundService> logger) : BackgroundService
{
    private readonly TimeSpan _crawlInterval = TimeSpan.FromHours(
        configuration.GetValue<int>("CustomFieldCrawler:IntervalHours", 24));

    private readonly TimeSpan _initialDelay = TimeSpan.FromMinutes(
        configuration.GetValue<int>("CustomFieldCrawler:InitialDelayMinutes", 1));

    /// <summary>
    /// Executes the background service.
    /// </summary>
    /// <param name="stoppingToken">Stopping token.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation(
            "Custom Field Crawler Background Service starting. Interval: {Interval}, Initial Delay: {Delay}",
            _crawlInterval,
            _initialDelay);

        // Wait for initial delay
        await Task.Delay(_initialDelay, stoppingToken);

        using var timer = new PeriodicTimer(_crawlInterval);

        try
        {
            // Run immediately on startup
            await RunCrawlAsync(stoppingToken);

            // Then run on schedule
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await RunCrawlAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Custom Field Crawler Background Service stopping");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Fatal error in Custom Field Crawler Background Service");
            throw;
        }
    }

    /// <summary>
    /// Runs a single crawl operation.
    /// </summary>
    private async Task RunCrawlAsync(CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Starting scheduled custom field crawl");

            // Create a scope to get scoped services
            using var scope = serviceProvider.CreateScope();
            var crawlerService = scope.ServiceProvider
                .GetRequiredService<ICustomFieldCrawlerService>();

            var result = await crawlerService.CrawlCustomFieldsAsync(cancellationToken);

            if (result.IsSuccess && result.Value != null)
            {
                logger.LogInformation(
                    "Scheduled crawl completed successfully: {Total} total, {New} new, {Updated} updated, {Stale} stale",
                    result.Value.TotalFields,
                    result.Value.NewFields,
                    result.Value.UpdatedFields,
                    result.Value.StaleFields);
            }
            else
            {
                logger.LogError("Scheduled crawl failed: {Error}", result.Error);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during scheduled custom field crawl");
        }
    }
}
