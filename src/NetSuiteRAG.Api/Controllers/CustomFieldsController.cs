using Microsoft.AspNetCore.Mvc;
using NetSuiteRAG.Api.Services.Interfaces;

namespace NetSuiteRAG.Api.Controllers;

/// <summary>
/// Controller for managing custom field crawling and metadata.
/// </summary>
[ApiController]
[Route("api/custom-fields")]
public class CustomFieldsController(
    ICustomFieldCrawlerService crawlerService,
    ILogger<CustomFieldsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets all custom field descriptors.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of custom field descriptors.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllCustomFields(
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await crawlerService.GetAllCustomFieldDescriptorsAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogError("Failed to get custom fields: {Error}", result.Error);
                return StatusCode(500, new { error = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting custom fields");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets custom field descriptors for a specific record type.
    /// </summary>
    /// <param name="recordType">Record type (e.g., "transaction", "customer").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of custom field descriptors for the record type.</returns>
    [HttpGet("{recordType}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetCustomFieldsByRecordType(
        string recordType,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await crawlerService.GetCustomFieldDescriptorsByRecordTypeAsync(
                recordType,
                cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogError("Failed to get custom fields for {RecordType}: {Error}", recordType, result.Error);
                return StatusCode(500, new { error = result.Error });
            }

            if (result.Value == null || result.Value.Count == 0)
            {
                return NotFound(new { error = $"No custom fields found for record type '{recordType}'" });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting custom fields for record type {RecordType}", recordType);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets stale custom fields (not seen in 90+ days).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of stale custom field descriptors.</returns>
    [HttpGet("stale")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStaleFields(
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await crawlerService.GetStaleFieldsAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogError("Failed to get stale fields: {Error}", result.Error);
                return StatusCode(500, new { error = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting stale fields");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets the change log for custom fields.
    /// </summary>
    /// <param name="limit">Maximum number of entries to return (default: 100).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of change log entries.</returns>
    [HttpGet("changes")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetChangeLog(
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await crawlerService.GetChangeLogAsync(limit, cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogError("Failed to get change log: {Error}", result.Error);
                return StatusCode(500, new { error = result.Error });
            }

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting change log");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Triggers a manual crawl of custom fields.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Crawl result summary.</returns>
    [HttpPost("crawl")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TriggerCrawl(
        CancellationToken cancellationToken)
    {
        try
        {
            logger.LogInformation("Manual custom field crawl triggered");

            var result = await crawlerService.CrawlCustomFieldsAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogError("Manual crawl failed: {Error}", result.Error);
                return StatusCode(500, new { error = result.Error });
            }

            logger.LogInformation(
                "Manual crawl completed: {Total} total, {New} new, {Updated} updated, {Stale} stale",
                result.Value?.TotalFields,
                result.Value?.NewFields,
                result.Value?.UpdatedFields,
                result.Value?.StaleFields);

            return Ok(new
            {
                message = "Crawl completed successfully",
                result = result.Value
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during manual crawl");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}
