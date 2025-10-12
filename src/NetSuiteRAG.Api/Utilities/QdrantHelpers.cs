using NetSuiteRAG.Api.Services.Interfaces;

namespace NetSuiteRAG.Api.Utilities;

/// <summary>
/// Helper utilities for working with Qdrant vector database.
/// Provides reusable methods for ID conversion, deduplication, and collection management.
/// </summary>
public static class QdrantHelpers
{
    /// <summary>
    /// Converts a Guid to a deterministic ulong point ID for Qdrant.
    /// Uses the first 8 bytes of the Guid to ensure consistency across multiple calls.
    /// </summary>
    /// <param name="guid">The Guid to convert.</param>
    /// <returns>A deterministic ulong point ID that will always be the same for the same Guid.</returns>
    /// <remarks>
    /// This method is preferred over GetHashCode() because:
    /// 1. GetHashCode() is not guaranteed to be stable across application restarts
    /// 2. GetHashCode() can produce collisions
    /// 3. This method uses the actual Guid bytes for a truly unique mapping
    /// </remarks>
    public static ulong ConvertGuidToPointId(Guid guid)
    {
        var bytes = guid.ToByteArray();
        return BitConverter.ToUInt64(bytes, 0);
    }

    /// <summary>
    /// Gets all existing point IDs from a Qdrant collection to avoid duplicate indexing.
    /// Handles all edge cases including non-existent collections and empty collections.
    /// </summary>
    /// <param name="vectorStore">The vector store service.</param>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="logger">Logger instance for diagnostics.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>A hash set of existing point IDs, or an empty set if collection doesn't exist.</returns>
    public static async Task<HashSet<ulong>> GetExistingPointIdsAsync(
        IVectorStoreService vectorStore,
        string collectionName,
        ILogger logger,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if collection exists first
            var exists = await vectorStore.CollectionExistsAsync(collectionName, cancellationToken);
            if (!exists.IsSuccess || !exists.Value)
            {
                logger.LogDebug(
                    "Collection {CollectionName} does not exist, returning empty ID set",
                    collectionName);
                return [];
            }

            // Get the count to optimize - if empty, no need to scroll
            var countResult = await vectorStore.GetVectorCountAsync(collectionName, cancellationToken);
            if (!countResult.IsSuccess || countResult.Value == 0)
            {
                logger.LogDebug(
                    "Collection {CollectionName} is empty, returning empty ID set",
                    collectionName);
                return [];
            }

            // Retrieve all point IDs using the Scroll API
            var result = await vectorStore.GetAllPointIdsAsync(collectionName, cancellationToken);

            if (!result.IsSuccess)
            {
                logger.LogWarning(
                    "Failed to get point IDs from {CollectionName}: {Error}. Assuming empty collection.",
                    collectionName, result.Error);
                return [];
            }

            logger.LogInformation(
                "Retrieved {Count} existing point IDs from {CollectionName}",
                result.Value!.Count, collectionName);

            return result.Value!;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Error getting existing point IDs from {CollectionName}, assuming empty collection",
                collectionName);
            return [];
        }
    }

    /// <summary>
    /// Filters a list of items to only include those not already indexed in Qdrant.
    /// </summary>
    /// <typeparam name="T">The type of items to filter.</typeparam>
    /// <param name="items">The list of items to filter.</param>
    /// <param name="existingIds">The set of existing point IDs in Qdrant.</param>
    /// <param name="idSelector">Function to extract the Guid from each item.</param>
    /// <returns>A list of items that are not yet indexed.</returns>
    public static List<T> FilterUnindexedItems<T>(
        List<T> items,
        HashSet<ulong> existingIds,
        Func<T, Guid> idSelector)
    {
        return items
            .Where(item => !existingIds.Contains(ConvertGuidToPointId(idSelector(item))))
            .ToList();
    }

    /// <summary>
    /// Converts a list of Guids to Qdrant point IDs.
    /// </summary>
    /// <param name="guids">The list of Guids to convert.</param>
    /// <returns>A list of ulong point IDs.</returns>
    public static List<ulong> ConvertGuidsToPointIds(List<Guid> guids)
    {
        return guids.Select(ConvertGuidToPointId).ToList();
    }

    /// <summary>
    /// Converts an IEnumerable of Guids to Qdrant point IDs.
    /// </summary>
    /// <param name="guids">The enumerable of Guids to convert.</param>
    /// <returns>A list of ulong point IDs.</returns>
    public static List<ulong> ConvertGuidsToPointIds(IEnumerable<Guid> guids)
    {
        return guids.Select(ConvertGuidToPointId).ToList();
    }
}
