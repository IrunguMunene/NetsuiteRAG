using Microsoft.AspNetCore.Mvc;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;
using NetSuiteRAG.Shared.Monitoring;

namespace NetSuiteRAG.Api.Controllers;

/// <summary>
/// API controller for managing business glossary terms and query exemplars.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class GlossaryController(
    IGlossaryService glossaryService,
    MetricsCollector metrics,
    ILogger<GlossaryController> logger) : ControllerBase
{
    // ========== Glossary Terms Endpoints ==========

    /// <summary>
    /// Gets all glossary terms, optionally filtered by category.
    /// </summary>
    /// <param name="category">Optional category filter.</param>
    /// <param name="activeOnly">Whether to return only active terms (default: true).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of glossary terms.</returns>
    [HttpGet("terms")]
    [ProducesResponseType(typeof(List<GlossaryTerm>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllTerms(
        [FromQuery] string? category = null,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("glossary.terms.get");

        logger.LogInformation("GET /api/glossary/terms - Category: {Category}, ActiveOnly: {ActiveOnly}",
            category ?? "All", activeOnly);

        var result = await glossaryService.GetAllTermsAsync(category, activeOnly, cancellationToken);

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        metrics.RecordLatency("glossary.terms.get", elapsedMs);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to retrieve glossary terms: {Error}", result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a glossary term by ID.
    /// </summary>
    /// <param name="id">Term ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The glossary term.</returns>
    [HttpGet("terms/{id:guid}")]
    [ProducesResponseType(typeof(GlossaryTerm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetTermById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("GET /api/glossary/terms/{TermId}", id);

        var result = await glossaryService.GetTermByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Glossary term not found: {TermId}", id);
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Searches glossary terms by keyword.
    /// </summary>
    /// <param name="keyword">Search keyword.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching glossary terms.</returns>
    [HttpGet("terms/search")]
    [ProducesResponseType(typeof(List<GlossaryTerm>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchTerms(
        [FromQuery] string keyword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest("Keyword is required");

        logger.LogInformation("GET /api/glossary/terms/search - Keyword: {Keyword}", keyword);

        var result = await glossaryService.SearchTermsAsync(keyword, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to search glossary terms: {Error}", result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new glossary term.
    /// </summary>
    /// <param name="request">Term creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created glossary term.</returns>
    [HttpPost("terms")]
    [ProducesResponseType(typeof(GlossaryTerm), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTerm(
        [FromBody] CreateGlossaryTermRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("POST /api/glossary/terms - Term: {Term}", request.Term);

        var result = await glossaryService.CreateTermAsync(
            request.Term,
            request.Definition,
            request.Synonyms ?? [],
            request.Category,
            request.Examples ?? [],
            cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create glossary term: {Error}", result.Error);
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            nameof(GetTermById),
            new { id = result.Value.Id },
            result.Value);
    }

    /// <summary>
    /// Updates an existing glossary term.
    /// </summary>
    /// <param name="id">Term ID.</param>
    /// <param name="request">Term update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated glossary term.</returns>
    [HttpPut("terms/{id:guid}")]
    [ProducesResponseType(typeof(GlossaryTerm), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateTerm(
        Guid id,
        [FromBody] UpdateGlossaryTermRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("PUT /api/glossary/terms/{TermId}", id);

        var result = await glossaryService.UpdateTermAsync(
            id,
            request.Term,
            request.Definition,
            request.Synonyms ?? [],
            request.Category,
            request.Examples ?? [],
            cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to update glossary term: {Error}", result.Error);

            if (result.Error.Contains("not found"))
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a glossary term (soft delete).
    /// </summary>
    /// <param name="id">Term ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    [HttpDelete("terms/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteTerm(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("DELETE /api/glossary/terms/{TermId}", id);

        var result = await glossaryService.DeleteTermAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to delete glossary term: {Error}", result.Error);

            if (result.Error.Contains("not found"))
                return NotFound(result.Error);

            return StatusCode(StatusCodes.Status500InternalServerError, result.Error);
        }

        return NoContent();
    }

    // ========== Query Exemplars Endpoints ==========

    /// <summary>
    /// Gets all query exemplars, optionally filtered by record type and difficulty.
    /// </summary>
    /// <param name="recordType">Optional record type filter.</param>
    /// <param name="difficulty">Optional difficulty filter.</param>
    /// <param name="activeOnly">Whether to return only active exemplars (default: true).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of query exemplars.</returns>
    [HttpGet("exemplars")]
    [ProducesResponseType(typeof(List<QueryExemplar>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAllExemplars(
        [FromQuery] string? recordType = null,
        [FromQuery] DifficultyLevel? difficulty = null,
        [FromQuery] bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("glossary.exemplars.get");

        logger.LogInformation("GET /api/glossary/exemplars - RecordType: {RecordType}, Difficulty: {Difficulty}, ActiveOnly: {ActiveOnly}",
            recordType ?? "All", difficulty?.ToString() ?? "All", activeOnly);

        var result = await glossaryService.GetAllExemplarsAsync(recordType, difficulty, activeOnly, cancellationToken);

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        metrics.RecordLatency("glossary.exemplars.get", elapsedMs);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to retrieve query exemplars: {Error}", result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Gets a query exemplar by ID.
    /// </summary>
    /// <param name="id">Exemplar ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The query exemplar.</returns>
    [HttpGet("exemplars/{id:guid}")]
    [ProducesResponseType(typeof(QueryExemplar), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetExemplarById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("GET /api/glossary/exemplars/{ExemplarId}", id);

        var result = await glossaryService.GetExemplarByIdAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogWarning("Query exemplar not found: {ExemplarId}", id);
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Searches query exemplars by keyword.
    /// </summary>
    /// <param name="keyword">Search keyword.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching query exemplars.</returns>
    [HttpGet("exemplars/search")]
    [ProducesResponseType(typeof(List<QueryExemplar>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> SearchExemplars(
        [FromQuery] string keyword,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(keyword))
            return BadRequest("Keyword is required");

        logger.LogInformation("GET /api/glossary/exemplars/search - Keyword: {Keyword}", keyword);

        var result = await glossaryService.SearchExemplarsAsync(keyword, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to search query exemplars: {Error}", result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Creates a new query exemplar.
    /// </summary>
    /// <param name="request">Exemplar creation request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The created query exemplar.</returns>
    [HttpPost("exemplars")]
    [ProducesResponseType(typeof(QueryExemplar), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateExemplar(
        [FromBody] CreateQueryExemplarRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("POST /api/glossary/exemplars - Query: {Query}", request.NaturalQuery);

        var result = await glossaryService.CreateExemplarAsync(
            request.NaturalQuery,
            request.SavedSearchPlanJson,
            request.Explanation,
            request.Tags ?? [],
            request.Difficulty,
            request.RecordType,
            cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to create query exemplar: {Error}", result.Error);
            return BadRequest(result.Error);
        }

        return CreatedAtAction(
            nameof(GetExemplarById),
            new { id = result.Value.Id },
            result.Value);
    }

    /// <summary>
    /// Updates an existing query exemplar.
    /// </summary>
    /// <param name="id">Exemplar ID.</param>
    /// <param name="request">Exemplar update request.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The updated query exemplar.</returns>
    [HttpPut("exemplars/{id:guid}")]
    [ProducesResponseType(typeof(QueryExemplar), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateExemplar(
        Guid id,
        [FromBody] UpdateQueryExemplarRequest request,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("PUT /api/glossary/exemplars/{ExemplarId}", id);

        var result = await glossaryService.UpdateExemplarAsync(
            id,
            request.NaturalQuery,
            request.SavedSearchPlanJson,
            request.Explanation,
            request.Tags ?? [],
            request.Difficulty,
            request.RecordType,
            cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to update query exemplar: {Error}", result.Error);

            if (result.Error.Contains("not found"))
                return NotFound(result.Error);

            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a query exemplar (soft delete).
    /// </summary>
    /// <param name="id">Exemplar ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success result.</returns>
    [HttpDelete("exemplars/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteExemplar(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("DELETE /api/glossary/exemplars/{ExemplarId}", id);

        var result = await glossaryService.DeleteExemplarAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to delete query exemplar: {Error}", result.Error);

            if (result.Error.Contains("not found"))
                return NotFound(result.Error);

            return StatusCode(StatusCodes.Status500InternalServerError, result.Error);
        }

        return NoContent();
    }

    // ========== Statistics Endpoints ==========

    /// <summary>
    /// Gets statistics about the glossary.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Glossary statistics.</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(GlossaryStatistics), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatistics(
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("GET /api/glossary/statistics");

        var result = await glossaryService.GetStatisticsAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            logger.LogError("Failed to retrieve statistics: {Error}", result.Error);
            return StatusCode(StatusCodes.Status500InternalServerError, result.Error);
        }

        return Ok(result.Value);
    }
}

// ========== Request DTOs ==========

/// <summary>
/// Request model for creating a glossary term.
/// </summary>
public record CreateGlossaryTermRequest
{
    public required string Term { get; init; }
    public required string Definition { get; init; }
    public List<string>? Synonyms { get; init; }
    public required string Category { get; init; }
    public List<string>? Examples { get; init; }
}

/// <summary>
/// Request model for updating a glossary term.
/// </summary>
public record UpdateGlossaryTermRequest
{
    public required string Term { get; init; }
    public required string Definition { get; init; }
    public List<string>? Synonyms { get; init; }
    public required string Category { get; init; }
    public List<string>? Examples { get; init; }
}

/// <summary>
/// Request model for creating a query exemplar.
/// </summary>
public record CreateQueryExemplarRequest
{
    public required string NaturalQuery { get; init; }
    public required string SavedSearchPlanJson { get; init; }
    public required string Explanation { get; init; }
    public List<string>? Tags { get; init; }
    public required DifficultyLevel Difficulty { get; init; }
    public required string RecordType { get; init; }
}

/// <summary>
/// Request model for updating a query exemplar.
/// </summary>
public record UpdateQueryExemplarRequest
{
    public required string NaturalQuery { get; init; }
    public required string SavedSearchPlanJson { get; init; }
    public required string Explanation { get; init; }
    public List<string>? Tags { get; init; }
    public required DifficultyLevel Difficulty { get; init; }
    public required string RecordType { get; init; }
}
