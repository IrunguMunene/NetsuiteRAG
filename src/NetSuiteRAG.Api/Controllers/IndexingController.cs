using Microsoft.AspNetCore.Mvc;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Monitoring;

namespace NetSuiteRAG.Api.Controllers;

/// <summary>
/// API endpoints for indexing artifacts to the vector database.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class IndexingController(
    IIndexingService indexingService,
    MetricsCollector metrics,
    ILogger<IndexingController> logger) : ControllerBase
{
    /// <summary>
    /// Indexes all field descriptors to the vector database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of field descriptors indexed.</returns>
    /// <response code="200">Field descriptors indexed successfully.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("field-descriptors")]
    [ProducesResponseType(typeof(IndexingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IndexFieldDescriptors(
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("api.indexing.field-descriptors.requests");

        logger.LogInformation("Indexing field descriptors via API");

        var result = await indexingService.IndexFieldDescriptorsAsync(cancellationToken);

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        metrics.RecordLatency("api.indexing.field-descriptors", elapsedMs);

        if (!result.IsSuccess)
        {
            metrics.IncrementCounter("api.indexing.field-descriptors.errors");
            logger.LogError("Failed to index field descriptors: {Error}", result.Error);
            return StatusCode(500, new { error = result.Error });
        }

        logger.LogInformation(
            "Indexed {Count} field descriptors in {ElapsedMs}ms",
            result.Value, elapsedMs);

        return Ok(new IndexingResult
        {
            Count = result.Value,
            ElapsedMs = elapsedMs,
            Message = $"Indexed {result.Value} field descriptors successfully"
        });
    }

    /// <summary>
    /// Indexes all glossary terms to the vector database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of glossary terms indexed.</returns>
    /// <response code="200">Glossary terms indexed successfully.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("glossary-terms")]
    [ProducesResponseType(typeof(IndexingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IndexGlossaryTerms(
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("api.indexing.glossary-terms.requests");

        logger.LogInformation("Indexing glossary terms via API");

        var result = await indexingService.IndexGlossaryTermsAsync(cancellationToken);

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        metrics.RecordLatency("api.indexing.glossary-terms", elapsedMs);

        if (!result.IsSuccess)
        {
            metrics.IncrementCounter("api.indexing.glossary-terms.errors");
            logger.LogError("Failed to index glossary terms: {Error}", result.Error);
            return StatusCode(500, new { error = result.Error });
        }

        logger.LogInformation(
            "Indexed {Count} glossary terms in {ElapsedMs}ms",
            result.Value, elapsedMs);

        return Ok(new IndexingResult
        {
            Count = result.Value,
            ElapsedMs = elapsedMs,
            Message = $"Indexed {result.Value} glossary terms successfully"
        });
    }

    /// <summary>
    /// Indexes all query exemplars to the vector database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Number of query exemplars indexed.</returns>
    /// <response code="200">Query exemplars indexed successfully.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("query-exemplars")]
    [ProducesResponseType(typeof(IndexingResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IndexQueryExemplars(
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("api.indexing.query-exemplars.requests");

        logger.LogInformation("Indexing query exemplars via API");

        var result = await indexingService.IndexQueryExemplarsAsync(cancellationToken);

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        metrics.RecordLatency("api.indexing.query-exemplars", elapsedMs);

        if (!result.IsSuccess)
        {
            metrics.IncrementCounter("api.indexing.query-exemplars.errors");
            logger.LogError("Failed to index query exemplars: {Error}", result.Error);
            return StatusCode(500, new { error = result.Error });
        }

        logger.LogInformation(
            "Indexed {Count} query exemplars in {ElapsedMs}ms",
            result.Value, elapsedMs);

        return Ok(new IndexingResult
        {
            Count = result.Value,
            ElapsedMs = elapsedMs,
            Message = $"Indexed {result.Value} query exemplars successfully"
        });
    }

    /// <summary>
    /// Indexes all artifacts (field descriptors, glossary terms, and query exemplars) to the vector database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Indexing statistics for all artifacts.</returns>
    /// <response code="200">All artifacts indexed successfully.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("all")]
    [ProducesResponseType(typeof(IndexingStatisticsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> IndexAll(
        CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("api.indexing.all.requests");

        logger.LogInformation("Indexing all artifacts via API");

        var result = await indexingService.IndexAllAsync(cancellationToken);

        var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
        metrics.RecordLatency("api.indexing.all", elapsedMs);

        if (!result.IsSuccess)
        {
            metrics.IncrementCounter("api.indexing.all.errors");
            logger.LogError("Failed to index all artifacts: {Error}", result.Error);
            return StatusCode(500, new { error = result.Error });
        }

        var stats = result.Value!;

        logger.LogInformation(
            "Indexed all artifacts: {Total} total in {ElapsedMs}ms " +
            "({FieldCount} field descriptors, {GlossaryCount} glossary terms, {ExemplarCount} query exemplars)",
            stats.TotalIndexed, elapsedMs,
            stats.FieldDescriptorsIndexed,
            stats.GlossaryTermsIndexed,
            stats.QueryExemplarsIndexed);

        return Ok(new IndexingStatisticsResponse
        {
            FieldDescriptorsIndexed = stats.FieldDescriptorsIndexed,
            GlossaryTermsIndexed = stats.GlossaryTermsIndexed,
            QueryExemplarsIndexed = stats.QueryExemplarsIndexed,
            TotalIndexed = stats.TotalIndexed,
            ElapsedMs = elapsedMs,
            Message = $"Indexed {stats.TotalIndexed} total artifacts successfully"
        });
    }

    /// <summary>
    /// Gets the current indexing status for all collections.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Indexing status information.</returns>
    /// <response code="200">Indexing status retrieved successfully.</response>
    /// <response code="500">Internal server error.</response>
    [HttpGet("status")]
    [ProducesResponseType(typeof(IndexingStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetStatus(
        CancellationToken cancellationToken)
    {
        metrics.IncrementCounter("api.indexing.status.requests");

        var result = await indexingService.GetIndexingStatusAsync(cancellationToken);

        if (!result.IsSuccess)
        {
            metrics.IncrementCounter("api.indexing.status.errors");
            logger.LogError("Failed to get indexing status: {Error}", result.Error);
            return StatusCode(500, new { error = result.Error });
        }

        var status = result.Value!;

        return Ok(new IndexingStatusResponse
        {
            FieldDescriptorsInDb = status.FieldDescriptorsInDb,
            FieldDescriptorsInVector = status.FieldDescriptorsInVector,
            GlossaryTermsInDb = status.GlossaryTermsInDb,
            GlossaryTermsInVector = status.GlossaryTermsInVector,
            QueryExemplarsInDb = status.QueryExemplarsInDb,
            QueryExemplarsInVector = status.QueryExemplarsInVector,
            CollectionsInitialized = status.CollectionsInitialized,
            IsUpToDate = status.IsUpToDate,
            Message = status.IsUpToDate ? "All collections are up to date" : "Indexing needed"
        });
    }

    /// <summary>
    /// Re-indexes a specific field descriptor by ID.
    /// </summary>
    /// <param name="id">The ID of the field descriptor to re-index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success indicator.</returns>
    /// <response code="200">Field descriptor re-indexed successfully.</response>
    /// <response code="404">Field descriptor not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("field-descriptors/{id}")]
    [ProducesResponseType(typeof(ReindexResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReindexFieldDescriptor(
        Guid id,
        CancellationToken cancellationToken)
    {
        metrics.IncrementCounter("api.indexing.reindex-field-descriptor.requests");

        logger.LogInformation("Re-indexing field descriptor {Id}", id);

        var result = await indexingService.ReindexFieldDescriptorAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            metrics.IncrementCounter("api.indexing.reindex-field-descriptor.errors");
            logger.LogError("Failed to re-index field descriptor {Id}: {Error}", id, result.Error);

            if (result.Error.Contains("not found"))
            {
                return NotFound(new { error = result.Error });
            }

            return StatusCode(500, new { error = result.Error });
        }

        logger.LogInformation("Re-indexed field descriptor {Id}", id);

        return Ok(new ReindexResult
        {
            Id = id,
            Message = "Field descriptor re-indexed successfully"
        });
    }

    /// <summary>
    /// Re-indexes a specific glossary term by ID.
    /// </summary>
    /// <param name="id">The ID of the glossary term to re-index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success indicator.</returns>
    /// <response code="200">Glossary term re-indexed successfully.</response>
    /// <response code="404">Glossary term not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("glossary-terms/{id}")]
    [ProducesResponseType(typeof(ReindexResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReindexGlossaryTerm(
        Guid id,
        CancellationToken cancellationToken)
    {
        metrics.IncrementCounter("api.indexing.reindex-glossary-term.requests");

        logger.LogInformation("Re-indexing glossary term {Id}", id);

        var result = await indexingService.ReindexGlossaryTermAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            metrics.IncrementCounter("api.indexing.reindex-glossary-term.errors");
            logger.LogError("Failed to re-index glossary term {Id}: {Error}", id, result.Error);

            if (result.Error.Contains("not found"))
            {
                return NotFound(new { error = result.Error });
            }

            return StatusCode(500, new { error = result.Error });
        }

        logger.LogInformation("Re-indexed glossary term {Id}", id);

        return Ok(new ReindexResult
        {
            Id = id,
            Message = "Glossary term re-indexed successfully"
        });
    }

    /// <summary>
    /// Re-indexes a specific query exemplar by ID.
    /// </summary>
    /// <param name="id">The ID of the query exemplar to re-index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success indicator.</returns>
    /// <response code="200">Query exemplar re-indexed successfully.</response>
    /// <response code="404">Query exemplar not found.</response>
    /// <response code="500">Internal server error.</response>
    [HttpPost("query-exemplars/{id}")]
    [ProducesResponseType(typeof(ReindexResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ReindexQueryExemplar(
        Guid id,
        CancellationToken cancellationToken)
    {
        metrics.IncrementCounter("api.indexing.reindex-query-exemplar.requests");

        logger.LogInformation("Re-indexing query exemplar {Id}", id);

        var result = await indexingService.ReindexQueryExemplarAsync(id, cancellationToken);

        if (!result.IsSuccess)
        {
            metrics.IncrementCounter("api.indexing.reindex-query-exemplar.errors");
            logger.LogError("Failed to re-index query exemplar {Id}: {Error}", id, result.Error);

            if (result.Error.Contains("not found"))
            {
                return NotFound(new { error = result.Error });
            }

            return StatusCode(500, new { error = result.Error });
        }

        logger.LogInformation("Re-indexed query exemplar {Id}", id);

        return Ok(new ReindexResult
        {
            Id = id,
            Message = "Query exemplar re-indexed successfully"
        });
    }
}

/// <summary>
/// Response model for indexing operations.
/// </summary>
public record IndexingResult
{
    public required int Count { get; init; }
    public required double ElapsedMs { get; init; }
    public required string Message { get; init; }
}

/// <summary>
/// Response model for bulk indexing operations.
/// </summary>
public record IndexingStatisticsResponse
{
    public required int FieldDescriptorsIndexed { get; init; }
    public required int GlossaryTermsIndexed { get; init; }
    public required int QueryExemplarsIndexed { get; init; }
    public required int TotalIndexed { get; init; }
    public required double ElapsedMs { get; init; }
    public required string Message { get; init; }
}

/// <summary>
/// Response model for indexing status.
/// </summary>
public record IndexingStatusResponse
{
    public required long FieldDescriptorsInDb { get; init; }
    public required long FieldDescriptorsInVector { get; init; }
    public required long GlossaryTermsInDb { get; init; }
    public required long GlossaryTermsInVector { get; init; }
    public required long QueryExemplarsInDb { get; init; }
    public required long QueryExemplarsInVector { get; init; }
    public required bool CollectionsInitialized { get; init; }
    public required bool IsUpToDate { get; init; }
    public required string Message { get; init; }
}

/// <summary>
/// Response model for re-indexing operations.
/// </summary>
public record ReindexResult
{
    public required Guid Id { get; init; }
    public required string Message { get; init; }
}
