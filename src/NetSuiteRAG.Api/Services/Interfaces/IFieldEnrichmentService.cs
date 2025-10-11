using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Interfaces;

/// <summary>
/// Service for enriching field definitions with aliases, business context, and embeddings.
/// </summary>
public interface IFieldEnrichmentService
{
    /// <summary>
    /// Enriches all field definitions that haven't been enriched yet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing enrichment statistics.</returns>
    Task<Result<EnrichmentResult>> EnrichAllFieldsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Enriches a specific field definition.
    /// </summary>
    /// <param name="recordType">The record type.</param>
    /// <param name="fieldId">The field ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> EnrichFieldAsync(
        string recordType,
        string fieldId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets enrichment status including count of enriched vs total fields.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing enrichment status.</returns>
    Task<Result<EnrichmentStatus>> GetEnrichmentStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Manually updates enrichment data for a specific field.
    /// </summary>
    /// <param name="recordType">The record type.</param>
    /// <param name="fieldId">The field ID.</param>
    /// <param name="aliases">List of aliases.</param>
    /// <param name="businessContext">Business context as JSON string.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> UpdateFieldEnrichmentAsync(
        string recordType,
        string fieldId,
        List<string>? aliases,
        string? businessContext,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of an enrichment operation.
/// </summary>
public record EnrichmentResult
{
    /// <summary>
    /// Total number of fields processed.
    /// </summary>
    public required int TotalProcessed { get; init; }

    /// <summary>
    /// Number of fields successfully enriched.
    /// </summary>
    public required int SuccessfullyEnriched { get; init; }

    /// <summary>
    /// Number of fields that failed enrichment.
    /// </summary>
    public required int Failed { get; init; }

    /// <summary>
    /// Number of fields skipped (already enriched).
    /// </summary>
    public required int Skipped { get; init; }
}

/// <summary>
/// Status of field enrichment.
/// </summary>
public record EnrichmentStatus
{
    /// <summary>
    /// Total number of fields.
    /// </summary>
    public required int TotalFields { get; init; }

    /// <summary>
    /// Number of enriched fields.
    /// </summary>
    public required int EnrichedFields { get; init; }

    /// <summary>
    /// Percentage of fields enriched.
    /// </summary>
    public required decimal EnrichmentPercentage { get; init; }

    /// <summary>
    /// Last enrichment run timestamp.
    /// </summary>
    public DateTime? LastEnrichmentRun { get; init; }
}
