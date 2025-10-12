using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Interfaces;

/// <summary>
/// Service for managing vector storage and retrieval in Qdrant.
/// </summary>
public interface IVectorStoreService
{
    /// <summary>
    /// Initializes all collections in the vector database.
    /// </summary>
    /// <param name="vectorSize">The dimension of embedding vectors (default: 768 for nomic-embed-text).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> InitializeCollectionsAsync(
        int vectorSize = 768,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a collection exists.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing true if collection exists, false otherwise.</returns>
    Task<Result<bool>> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the count of vectors in a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing the count of vectors.</returns>
    Task<Result<long>> GetVectorCountAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts a single vector point into a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="pointId">Unique ID for the point (maps to database ID).</param>
    /// <param name="vector">The embedding vector.</param>
    /// <param name="payload">Metadata payload for the point.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> UpsertVectorAsync(
        string collectionName,
        ulong pointId,
        float[] vector,
        Dictionary<string, object> payload,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Upserts multiple vector points into a collection in batch.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="pointIds">List of unique IDs for the points.</param>
    /// <param name="vectors">List of embedding vectors.</param>
    /// <param name="payloads">List of metadata payloads for the points.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> UpsertVectorsBatchAsync(
        string collectionName,
        List<ulong> pointIds,
        List<float[]> vectors,
        List<Dictionary<string, object>> payloads,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches for similar vectors in a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="queryVector">The query embedding vector.</param>
    /// <param name="limit">Maximum number of results to return (default: 5).</param>
    /// <param name="scoreThreshold">Minimum similarity score threshold (default: 0.0).</param>
    /// <param name="filter">Optional metadata filter.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing list of search results with scores and payloads.</returns>
    Task<Result<List<VectorSearchResult>>> SearchSimilarAsync(
        string collectionName,
        float[] queryVector,
        int limit = 5,
        float scoreThreshold = 0.0f,
        Dictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a vector point from a collection by ID.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="pointId">The ID of the point to delete.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> DeleteVectorAsync(
        string collectionName,
        ulong pointId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all vectors from a collection.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating success or failure.</returns>
    Task<Result<bool>> ClearCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests connection to Qdrant service.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result indicating connection status.</returns>
    Task<Result<bool>> TestConnectionAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all point IDs from a collection for deduplication purposes.
    /// Uses Qdrant's Scroll API to efficiently retrieve all point IDs without fetching vectors.
    /// </summary>
    /// <param name="collectionName">Name of the collection.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result containing a hash set of all point IDs in the collection.</returns>
    Task<Result<HashSet<ulong>>> GetAllPointIdsAsync(
        string collectionName,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents a vector search result from Qdrant.
/// </summary>
public record VectorSearchResult
{
    /// <summary>
    /// Gets the point ID.
    /// </summary>
    public required ulong Id { get; init; }

    /// <summary>
    /// Gets the similarity score (higher is more similar).
    /// </summary>
    public required float Score { get; init; }

    /// <summary>
    /// Gets the metadata payload.
    /// </summary>
    public required Dictionary<string, object> Payload { get; init; }
}
