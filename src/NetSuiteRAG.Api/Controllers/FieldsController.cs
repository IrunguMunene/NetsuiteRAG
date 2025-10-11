using Microsoft.AspNetCore.Mvc;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Controllers;

/// <summary>
/// API controller for NetSuite field definition management.
/// Provides endpoints for field lookups, search, and validation.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class FieldsController(
    IFieldDictionaryService fieldService,
    ILogger<FieldsController> logger) : ControllerBase
{
    /// <summary>
    /// Gets all field definitions for a specific record type.
    /// </summary>
    /// <param name="recordType">NetSuite record type (e.g., "transaction", "customer", "vendor").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of field definitions for the record type.</returns>
    /// <response code="200">Successfully retrieved field definitions.</response>
    /// <response code="400">Invalid record type.</response>
    [HttpGet("{recordType}")]
    [ProducesResponseType(typeof(List<FieldDefinition>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<FieldDefinition>>> GetFieldsForRecordType(
        string recordType,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordType))
            {
                return BadRequest(new { error = "Record type is required" });
            }

            logger.LogInformation(
                "Retrieving all fields for record type {RecordType}",
                recordType);

            var fields = await fieldService.GetFieldsForRecordTypeAsync(
                recordType,
                cancellationToken);

            return Ok(fields);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error retrieving fields for record type {RecordType}",
                recordType);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets a specific field definition by record type and field ID.
    /// </summary>
    /// <param name="recordType">NetSuite record type.</param>
    /// <param name="fieldId">Field identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Field definition if found.</returns>
    /// <response code="200">Field definition found.</response>
    /// <response code="404">Field not found.</response>
    [HttpGet("{recordType}/{fieldId}")]
    [ProducesResponseType(typeof(FieldDefinition), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FieldDefinition>> GetField(
        string recordType,
        string fieldId,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordType) || string.IsNullOrWhiteSpace(fieldId))
            {
                return BadRequest(new { error = "Record type and field ID are required" });
            }

            logger.LogInformation(
                "Retrieving field {FieldId} for record type {RecordType}",
                fieldId,
                recordType);

            var field = await fieldService.GetFieldAsync(
                recordType,
                fieldId,
                cancellationToken);

            if (field == null)
            {
                return NotFound(new
                {
                    error = $"Field '{fieldId}' not found in record type '{recordType}'"
                });
            }

            return Ok(field);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error retrieving field {FieldId} for record type {RecordType}",
                fieldId,
                recordType);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Searches for fields by label or alias within a record type.
    /// </summary>
    /// <param name="recordType">NetSuite record type to search within.</param>
    /// <param name="query">Search query (e.g., "vendor", "date", "amount").</param>
    /// <param name="maxResults">Maximum number of results to return (default: 10, max: 50).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching field definitions ordered by relevance.</returns>
    /// <response code="200">Successfully retrieved search results.</response>
    /// <response code="400">Invalid search parameters.</response>
    [HttpGet("{recordType}/search")]
    [ProducesResponseType(typeof(List<FieldDefinition>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<FieldDefinition>>> SearchFields(
        string recordType,
        [FromQuery] string query,
        [FromQuery] int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordType))
            {
                return BadRequest(new { error = "Record type is required" });
            }

            if (string.IsNullOrWhiteSpace(query))
            {
                return BadRequest(new { error = "Search query is required" });
            }

            if (maxResults <= 0 || maxResults > 50)
            {
                return BadRequest(new { error = "Max results must be between 1 and 50" });
            }

            logger.LogInformation(
                "Searching fields in {RecordType} for query '{Query}' (max: {MaxResults})",
                recordType,
                query,
                maxResults);

            var fields = await fieldService.SearchFieldsAsync(
                recordType,
                query,
                maxResults,
                cancellationToken);

            return Ok(fields);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error searching fields in {RecordType} for query '{Query}'",
                recordType,
                query);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Gets fields available via a specific join.
    /// </summary>
    /// <param name="recordType">Base record type (e.g., "transaction").</param>
    /// <param name="joinName">Join name (e.g., "vendorJoin", "customerJoin").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of field definitions accessible via the join.</returns>
    /// <response code="200">Successfully retrieved join fields.</response>
    /// <response code="400">Invalid parameters.</response>
    [HttpGet("{recordType}/joins/{joinName}")]
    [ProducesResponseType(typeof(List<FieldDefinition>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<List<FieldDefinition>>> GetFieldsForJoin(
        string recordType,
        string joinName,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordType) || string.IsNullOrWhiteSpace(joinName))
            {
                return BadRequest(new { error = "Record type and join name are required" });
            }

            logger.LogInformation(
                "Retrieving fields for join {JoinName} in {RecordType}",
                joinName,
                recordType);

            var fields = await fieldService.GetFieldsForJoinAsync(
                recordType,
                joinName,
                cancellationToken);

            return Ok(fields);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error retrieving fields for join {JoinName} in {RecordType}",
                joinName,
                recordType);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Validates whether a field exists and supports the specified operator.
    /// </summary>
    /// <param name="recordType">NetSuite record type.</param>
    /// <param name="fieldId">Field identifier.</param>
    /// <param name="operatorType">Operator to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with success status and error message if invalid.</returns>
    /// <response code="200">Validation result.</response>
    /// <response code="400">Invalid parameters.</response>
    [HttpGet("{recordType}/{fieldId}/validate/{operatorType}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ValidateFieldOperator(
        string recordType,
        string fieldId,
        string operatorType,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordType) ||
                string.IsNullOrWhiteSpace(fieldId) ||
                string.IsNullOrWhiteSpace(operatorType))
            {
                return BadRequest(new
                {
                    error = "Record type, field ID, and operator type are required"
                });
            }

            if (!Enum.TryParse<OperatorType>(operatorType, true, out var op))
            {
                return BadRequest(new
                {
                    error = $"Invalid operator type: {operatorType}"
                });
            }

            logger.LogInformation(
                "Validating operator {OperatorType} for field {FieldId} in {RecordType}",
                operatorType,
                fieldId,
                recordType);

            var (isValid, errorMessage) = await fieldService.ValidateFieldOperatorAsync(
                recordType,
                fieldId,
                op,
                cancellationToken);

            return Ok(new
            {
                isValid,
                errorMessage
            });
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error validating operator {OperatorType} for field {FieldId} in {RecordType}",
                operatorType,
                fieldId,
                recordType);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }

    /// <summary>
    /// Invalidates the cache for a specific record type.
    /// Used when custom fields are updated.
    /// </summary>
    /// <param name="recordType">Record type to invalidate cache for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success status.</returns>
    /// <response code="200">Cache invalidated successfully.</response>
    /// <response code="400">Invalid record type.</response>
    [HttpDelete("{recordType}/cache")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> InvalidateCache(
        string recordType,
        CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(recordType))
            {
                return BadRequest(new { error = "Record type is required" });
            }

            logger.LogInformation(
                "Invalidating cache for record type {RecordType}",
                recordType);

            await fieldService.InvalidateCacheAsync(recordType, cancellationToken);

            return Ok(new
            {
                message = $"Cache invalidated for record type '{recordType}'"
            });
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Error invalidating cache for record type {RecordType}",
                recordType);

            return StatusCode(
                StatusCodes.Status500InternalServerError,
                new { error = "Internal server error" });
        }
    }
}
