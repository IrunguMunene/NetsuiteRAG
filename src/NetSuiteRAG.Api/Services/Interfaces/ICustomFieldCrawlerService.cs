using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Interfaces;

/// <summary>
/// Service for crawling and tracking NetSuite custom fields.
/// </summary>
public interface ICustomFieldCrawlerService
{
    /// <summary>
    /// Crawls NetSuite for custom fields and updates the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with count of fields processed.</returns>
    Task<Result<CrawlResult>> CrawlCustomFieldsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all custom field descriptors from the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of custom field descriptors.</returns>
    Task<Result<List<CustomFieldDescriptor>>> GetAllCustomFieldDescriptorsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets custom field descriptors for a specific record type.
    /// </summary>
    /// <param name="recordType">Record type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of custom field descriptors.</returns>
    Task<Result<List<CustomFieldDescriptor>>> GetCustomFieldDescriptorsByRecordTypeAsync(
        string recordType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the change log for custom fields.
    /// </summary>
    /// <param name="limit">Maximum number of entries to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of change log entries.</returns>
    Task<Result<List<CustomFieldChangeLog>>> GetChangeLogAsync(
        int limit = 100,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets stale custom fields (not seen in 90+ days).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of stale custom field descriptors.</returns>
    Task<Result<List<CustomFieldDescriptor>>> GetStaleFieldsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Result of a custom field crawl operation.
/// </summary>
public record CrawlResult(
    int TotalFields,
    int NewFields,
    int UpdatedFields,
    int StaleFields,
    DateTime CrawlStartedAt,
    DateTime CrawlCompletedAt
);
