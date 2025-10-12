using Microsoft.AspNetCore.Mvc;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;
using NetSuiteRAG.Shared.Monitoring;

namespace NetSuiteRAG.Api.Controllers;

/// <summary>
/// API endpoints for validating SavedSearchPlan schemas.
/// Provides Layer 1-3 validation: structure, NetSuite schema, and operator compatibility.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class ValidationController(
    ISchemaValidationService schemaValidationService,
    MetricsCollector metrics,
    ILogger<ValidationController> logger) : ControllerBase
{
    /// <summary>
    /// Validates a SavedSearchPlan schema.
    /// Performs comprehensive validation including structure, field existence, and operator compatibility.
    /// </summary>
    /// <param name="plan">The SavedSearchPlan to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with success status and error details.</returns>
    /// <response code="200">Validation completed (check isValid field for success/failure).</response>
    /// <response code="400">Invalid request (malformed JSON).</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("schema")]
    [ProducesResponseType(typeof(ValidationResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ValidateSchema(
        [FromBody] SavedSearchPlan plan,
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("api.validation.schema.requests");

        logger.LogInformation(
            "Validating SavedSearchPlan schema for recordType: {RecordType}",
            plan?.RecordType ?? "null");

        if (plan == null)
        {
            metrics.IncrementCounter("api.validation.schema.errors");
            return BadRequest(new { error = "Request body cannot be null" });
        }

        try
        {
            // Run validation (synchronous wrapper around async operations)
            var result = await Task.Run(() => schemaValidationService.ValidateSchema(plan), cancellationToken);

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("api.validation.schema", elapsedMs);

            if (result.IsValid)
            {
                metrics.IncrementCounter("api.validation.schema.success");
                logger.LogInformation(
                    "Schema validation succeeded for recordType: {RecordType} in {ElapsedMs}ms",
                    plan.RecordType, elapsedMs);
            }
            else
            {
                metrics.IncrementCounter("api.validation.schema.validation-failed");
                logger.LogWarning(
                    "Schema validation failed for recordType: {RecordType} with {ErrorCount} errors in {ElapsedMs}ms",
                    plan.RecordType, result.Errors.Count, elapsedMs);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("api.validation.schema.errors");
            logger.LogError(ex, "Error validating SavedSearchPlan schema");
            return StatusCode(500, new { error = "Internal server error during validation" });
        }
    }

    /// <summary>
    /// Validates a single filter object.
    /// </summary>
    /// <param name="filter">The filter to validate.</param>
    /// <returns>List of validation errors (empty if valid).</returns>
    /// <response code="200">Validation completed.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPost("filter")]
    [ProducesResponseType(typeof(FilterValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ValidateFilter([FromBody] Filter filter)
    {
        metrics.IncrementCounter("api.validation.filter.requests");

        if (filter == null)
        {
            return BadRequest(new { error = "Request body cannot be null" });
        }

        var errors = schemaValidationService.ValidateFilter(filter, 0);

        return Ok(new FilterValidationResponse
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Message = errors.Count == 0 ? "Filter is valid" : $"Found {errors.Count} validation error(s)"
        });
    }

    /// <summary>
    /// Validates a single column object.
    /// </summary>
    /// <param name="column">The column to validate.</param>
    /// <returns>List of validation errors (empty if valid).</returns>
    /// <response code="200">Validation completed.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPost("column")]
    [ProducesResponseType(typeof(ColumnValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ValidateColumn([FromBody] Column column)
    {
        metrics.IncrementCounter("api.validation.column.requests");

        if (column == null)
        {
            return BadRequest(new { error = "Request body cannot be null" });
        }

        var errors = schemaValidationService.ValidateColumn(column, 0);

        return Ok(new ColumnValidationResponse
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Message = errors.Count == 0 ? "Column is valid" : $"Found {errors.Count} validation error(s)"
        });
    }

    /// <summary>
    /// Validates a single sort object.
    /// </summary>
    /// <param name="sort">The sort to validate.</param>
    /// <returns>List of validation errors (empty if valid).</returns>
    /// <response code="200">Validation completed.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPost("sort")]
    [ProducesResponseType(typeof(SortValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ValidateSort([FromBody] Sort sort)
    {
        metrics.IncrementCounter("api.validation.sort.requests");

        if (sort == null)
        {
            return BadRequest(new { error = "Request body cannot be null" });
        }

        var errors = schemaValidationService.ValidateSort(sort, 0);

        return Ok(new SortValidationResponse
        {
            IsValid = errors.Count == 0,
            Errors = errors,
            Message = errors.Count == 0 ? "Sort is valid" : $"Found {errors.Count} validation error(s)"
        });
    }

    /// <summary>
    /// Validates a record type is not null or empty.
    /// </summary>
    /// <param name="request">Record type validation request.</param>
    /// <returns>Validation result.</returns>
    /// <response code="200">Validation completed.</response>
    /// <response code="400">Invalid request.</response>
    [HttpPost("record-type")]
    [ProducesResponseType(typeof(RecordTypeValidationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult ValidateRecordType([FromBody] RecordTypeValidationRequest request)
    {
        metrics.IncrementCounter("api.validation.record-type.requests");

        if (request == null)
        {
            return BadRequest(new { error = "Request body cannot be null" });
        }

        var error = schemaValidationService.ValidateRecordType(request.RecordType);

        return Ok(new RecordTypeValidationResponse
        {
            IsValid = error == null,
            Error = error,
            Message = error == null ? "Record type is valid" : error.Message
        });
    }
}

/// <summary>
/// Response model for filter validation.
/// </summary>
public record FilterValidationResponse
{
    public required bool IsValid { get; init; }
    public required List<ValidationError> Errors { get; init; }
    public required string Message { get; init; }
}

/// <summary>
/// Response model for column validation.
/// </summary>
public record ColumnValidationResponse
{
    public required bool IsValid { get; init; }
    public required List<ValidationError> Errors { get; init; }
    public required string Message { get; init; }
}

/// <summary>
/// Response model for sort validation.
/// </summary>
public record SortValidationResponse
{
    public required bool IsValid { get; init; }
    public required List<ValidationError> Errors { get; init; }
    public required string Message { get; init; }
}

/// <summary>
/// Request model for record type validation.
/// </summary>
public record RecordTypeValidationRequest
{
    public required string RecordType { get; init; }
}

/// <summary>
/// Response model for record type validation.
/// </summary>
public record RecordTypeValidationResponse
{
    public required bool IsValid { get; init; }
    public ValidationError? Error { get; init; }
    public required string Message { get; init; }
}
