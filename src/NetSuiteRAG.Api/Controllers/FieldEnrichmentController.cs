using Microsoft.AspNetCore.Mvc;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Monitoring;

namespace NetSuiteRAG.Api.Controllers;

/// <summary>
/// Controller for managing field enrichment operations.
/// </summary>
[ApiController]
[Route("api/enrichment")]
public class FieldEnrichmentController(
    IFieldEnrichmentService enrichmentService,
    MetricsCollector metrics,
    ILogger<FieldEnrichmentController> logger) : ControllerBase
{
    /// <summary>
    /// Gets the current enrichment status.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Enrichment status including percentage and last run time.</returns>
    [HttpGet("status")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatus(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("enrichment.status.requests");

        try
        {
            var result = await enrichmentService.GetEnrichmentStatusAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                metrics.IncrementCounter("enrichment.status.errors");
                logger.LogError("Failed to get enrichment status: {Error}", result.Error);
                return StatusCode(500, new { error = result.Error });
            }

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("enrichment.status", elapsedMs);

            return Ok(result.Value);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("enrichment.status.errors");
            logger.LogError(ex, "Error getting enrichment status");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Triggers manual enrichment of all unenriched fields.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Enrichment result with statistics.</returns>
    [HttpPost("trigger")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> TriggerEnrichment(CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("enrichment.trigger.requests");

        try
        {
            logger.LogInformation("Manual field enrichment triggered");

            var result = await enrichmentService.EnrichAllFieldsAsync(cancellationToken);

            if (!result.IsSuccess)
            {
                metrics.IncrementCounter("enrichment.trigger.errors");
                logger.LogError("Manual enrichment failed: {Error}", result.Error);
                return StatusCode(500, new { error = result.Error });
            }

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("enrichment.trigger", elapsedMs);

            logger.LogInformation(
                "Manual enrichment completed: {Total} total, {Success} successful, {Failed} failed",
                result.Value?.TotalProcessed,
                result.Value?.SuccessfullyEnriched,
                result.Value?.Failed);

            return Ok(new
            {
                message = "Enrichment completed successfully",
                result = result.Value
            });
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("enrichment.trigger.errors");
            logger.LogError(ex, "Error during manual enrichment");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets enrichment data for a specific field.
    /// </summary>
    /// <param name="recordType">The record type.</param>
    /// <param name="fieldId">The field ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Field enrichment data.</returns>
    [HttpGet("field/{recordType}/{fieldId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public Task<IActionResult> GetFieldEnrichment(
        string recordType,
        string fieldId,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("enrichment.get-field.requests");

        try
        {
            // This would require a new method in IFieldEnrichmentService
            // For now, return a simple response
            logger.LogInformation(
                "Getting enrichment for field {RecordType}.{FieldId}",
                recordType, fieldId);

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("enrichment.get-field", elapsedMs);

            return Task.FromResult<IActionResult>(Ok(new
            {
                recordType,
                fieldId,
                message = "Field enrichment data retrieval - to be implemented"
            }));
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("enrichment.get-field.errors");
            logger.LogError(ex, "Error getting field enrichment");
            return Task.FromResult<IActionResult>(StatusCode(500, new { error = "Internal server error" }));
        }
    }

    /// <summary>
    /// Manually updates enrichment data for a specific field.
    /// </summary>
    /// <param name="recordType">The record type.</param>
    /// <param name="fieldId">The field ID.</param>
    /// <param name="request">The enrichment update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    [HttpPut("field/{recordType}/{fieldId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateFieldEnrichment(
        string recordType,
        string fieldId,
        [FromBody] EnrichmentUpdateRequest request,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("enrichment.update-field.requests");

        try
        {
            if (string.IsNullOrWhiteSpace(recordType) || string.IsNullOrWhiteSpace(fieldId))
            {
                return BadRequest(new { error = "Record type and field ID are required" });
            }

            logger.LogInformation(
                "Updating enrichment for field {RecordType}.{FieldId}",
                recordType, fieldId);

            var result = await enrichmentService.UpdateFieldEnrichmentAsync(
                recordType,
                fieldId,
                request.Aliases,
                request.BusinessContext,
                cancellationToken);

            if (!result.IsSuccess)
            {
                metrics.IncrementCounter("enrichment.update-field.errors");
                logger.LogError(
                    "Failed to update enrichment for {RecordType}.{FieldId}: {Error}",
                    recordType, fieldId, result.Error);
                return StatusCode(500, new { error = result.Error });
            }

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("enrichment.update-field", elapsedMs);

            return Ok(new { message = "Field enrichment updated successfully" });
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("enrichment.update-field.errors");
            logger.LogError(ex, "Error updating field enrichment");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}

/// <summary>
/// Request model for updating field enrichment.
/// </summary>
public record EnrichmentUpdateRequest
{
    /// <summary>
    /// List of aliases to add.
    /// </summary>
    public List<string>? Aliases { get; init; }

    /// <summary>
    /// Business context as JSON string.
    /// </summary>
    public string? BusinessContext { get; init; }
}
