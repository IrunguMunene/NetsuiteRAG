using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Interfaces;

/// <summary>
/// Service for managing and retrieving NetSuite field definitions.
/// Provides caching and search capabilities for field metadata used in query planning.
/// </summary>
public interface IFieldDictionaryService
{
    /// <summary>
    /// Retrieves a specific field definition by record type and field ID.
    /// Results are cached in Redis for fast subsequent lookups.
    /// </summary>
    /// <param name="recordType">NetSuite record type (e.g., "transaction", "customer").</param>
    /// <param name="fieldId">Field identifier (e.g., "trandate", "entity").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Field definition if found; otherwise, null.</returns>
    Task<FieldDefinition?> GetFieldAsync(
        string recordType,
        string fieldId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all field definitions for a specific record type.
    /// Includes both standard and custom fields.
    /// Results are cached in Redis.
    /// </summary>
    /// <param name="recordType">NetSuite record type (e.g., "transaction", "customer").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of field definitions for the record type.</returns>
    Task<List<FieldDefinition>> GetFieldsForRecordTypeAsync(
        string recordType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for field definitions by label or alias using fuzzy matching.
    /// Used during natural language query planning to map user terms to field IDs.
    /// </summary>
    /// <param name="recordType">NetSuite record type to search within.</param>
    /// <param name="searchQuery">Search term (e.g., "vendor", "date", "amount").</param>
    /// <param name="maxResults">Maximum number of results to return (default: 10).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching field definitions, ordered by relevance.</returns>
    Task<List<FieldDefinition>> SearchFieldsAsync(
        string recordType,
        string searchQuery,
        int maxResults = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves field definitions for a specific join.
    /// Used to determine which fields are available when a join is added to a query plan.
    /// </summary>
    /// <param name="recordType">Base record type (e.g., "transaction").</param>
    /// <param name="joinName">Join name (e.g., "vendorJoin", "customerJoin").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of field definitions accessible via the join.</returns>
    Task<List<FieldDefinition>> GetFieldsForJoinAsync(
        string recordType,
        string joinName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validates whether a field exists and is compatible with the specified operator.
    /// Used during query plan validation (Layer 2 and 3).
    /// </summary>
    /// <param name="recordType">NetSuite record type.</param>
    /// <param name="fieldId">Field identifier.</param>
    /// <param name="operatorType">Operator to validate.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Validation result with success status and error message if invalid.</returns>
    Task<(bool IsValid, string? ErrorMessage)> ValidateFieldOperatorAsync(
        string recordType,
        string fieldId,
        OperatorType operatorType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates the cache for a specific record type.
    /// Used when custom fields are updated via the nightly crawler.
    /// </summary>
    /// <param name="recordType">Record type to invalidate cache for.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    Task InvalidateCacheAsync(
        string recordType,
        CancellationToken cancellationToken = default);
}
