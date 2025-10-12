using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Interfaces;

/// <summary>
/// Service for indexing artifacts to the vector database.
/// </summary>
public interface IIndexingService
{
    /// <summary>
    /// Indexes all field descriptors to the vector database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the number of field descriptors indexed.</returns>
    Task<Result<int>> IndexFieldDescriptorsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes all glossary terms to the vector database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the number of glossary terms indexed.</returns>
    Task<Result<int>> IndexGlossaryTermsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes all query exemplars to the vector database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the number of query exemplars indexed.</returns>
    Task<Result<int>> IndexQueryExemplarsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Indexes all artifacts (field descriptors, glossary terms, and query exemplars) to the vector database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the total number of artifacts indexed.</returns>
    Task<Result<IndexingStatistics>> IndexAllAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the current indexing status for all collections.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing indexing status information.</returns>
    Task<Result<IndexingStatus>> GetIndexingStatusAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-indexes a specific field descriptor by ID.
    /// </summary>
    /// <param name="fieldDescriptorId">The ID of the field descriptor to re-index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> ReindexFieldDescriptorAsync(
        Guid fieldDescriptorId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-indexes a specific glossary term by ID.
    /// </summary>
    /// <param name="glossaryTermId">The ID of the glossary term to re-index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> ReindexGlossaryTermAsync(
        Guid glossaryTermId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-indexes a specific query exemplar by ID.
    /// </summary>
    /// <param name="queryExemplarId">The ID of the query exemplar to re-index.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> ReindexQueryExemplarAsync(
        Guid queryExemplarId,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics from indexing operation.
/// </summary>
public record IndexingStatistics
{
    /// <summary>
    /// Gets the number of field descriptors indexed.
    /// </summary>
    public required int FieldDescriptorsIndexed { get; init; }

    /// <summary>
    /// Gets the number of glossary terms indexed.
    /// </summary>
    public required int GlossaryTermsIndexed { get; init; }

    /// <summary>
    /// Gets the number of query exemplars indexed.
    /// </summary>
    public required int QueryExemplarsIndexed { get; init; }

    /// <summary>
    /// Gets the total number of artifacts indexed.
    /// </summary>
    public int TotalIndexed => FieldDescriptorsIndexed + GlossaryTermsIndexed + QueryExemplarsIndexed;

    /// <summary>
    /// Gets the elapsed time in milliseconds.
    /// </summary>
    public required double ElapsedMs { get; init; }
}

/// <summary>
/// Current status of indexing in vector database.
/// </summary>
public record IndexingStatus
{
    /// <summary>
    /// Gets the number of field descriptors in the database.
    /// </summary>
    public required long FieldDescriptorsInDb { get; init; }

    /// <summary>
    /// Gets the number of field descriptors in the vector store.
    /// </summary>
    public required long FieldDescriptorsInVector { get; init; }

    /// <summary>
    /// Gets the number of glossary terms in the database.
    /// </summary>
    public required long GlossaryTermsInDb { get; init; }

    /// <summary>
    /// Gets the number of glossary terms in the vector store.
    /// </summary>
    public required long GlossaryTermsInVector { get; init; }

    /// <summary>
    /// Gets the number of query exemplars in the database.
    /// </summary>
    public required long QueryExemplarsInDb { get; init; }

    /// <summary>
    /// Gets the number of query exemplars in the vector store.
    /// </summary>
    public required long QueryExemplarsInVector { get; init; }

    /// <summary>
    /// Gets whether collections are initialized.
    /// </summary>
    public required bool CollectionsInitialized { get; init; }

    /// <summary>
    /// Gets whether indexing is up to date.
    /// </summary>
    public bool IsUpToDate =>
        FieldDescriptorsInDb == FieldDescriptorsInVector &&
        GlossaryTermsInDb == GlossaryTermsInVector &&
        QueryExemplarsInDb == QueryExemplarsInVector;
}
