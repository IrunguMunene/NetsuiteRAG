using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;
using NetSuiteRAG.Shared.Monitoring;
using Qdrant.Client;
using Qdrant.Client.Grpc;

namespace NetSuiteRAG.Api.Services.Implementations;

/// <summary>
/// Service for managing vector storage and retrieval in Qdrant.
/// </summary>
public class QdrantVectorService(
    IConfiguration configuration,
    MetricsCollector metrics,
    ILogger<QdrantVectorService> logger) : IVectorStoreService
{
    private const string FieldDescriptorsCollection = "field_descriptors";
    private const string GlossaryTermsCollection = "glossary_terms";
    private const string QueryExemplarsCollection = "query_exemplars";

    private QdrantClient? _client;

    /// <summary>
    /// Gets the Qdrant client, initializing it if necessary.
    /// </summary>
    private QdrantClient GetClient()
    {
        if (_client != null)
        {
            return _client;
        }

        var host = configuration["Qdrant:Host"] ?? "localhost";
        var port = int.Parse(configuration["Qdrant:Port"] ?? "6334"); // gRPC port
        var useHttps = bool.Parse(configuration["Qdrant:UseHttps"] ?? "false");
        var apiKey = configuration["Qdrant:ApiKey"];

        logger.LogInformation("Connecting to Qdrant at {Host}:{Port} (HTTPS: {UseHttps})",
            host, port, useHttps);

        _client = new QdrantClient(
            host: host,
            port: port,
            https: useHttps,
            apiKey: apiKey);

        return _client;
    }

    /// <summary>
    /// Initializes all collections in the vector database.
    /// </summary>
    public async Task<Result<bool>> InitializeCollectionsAsync(
        int vectorSize = 768,
        CancellationToken cancellationToken = default)
    {
        metrics.IncrementCounter("qdrant.initialize.requests");
        var startTime = DateTime.UtcNow;

        try
        {
            var client = GetClient();

            // Create field_descriptors collection
            await EnsureCollectionExistsAsync(
                client, FieldDescriptorsCollection, vectorSize, cancellationToken);

            // Create glossary_terms collection
            await EnsureCollectionExistsAsync(
                client, GlossaryTermsCollection, vectorSize, cancellationToken);

            // Create query_exemplars collection
            await EnsureCollectionExistsAsync(
                client, QueryExemplarsCollection, vectorSize, cancellationToken);

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("qdrant.initialize", elapsedMs);

            logger.LogInformation(
                "Successfully initialized all Qdrant collections in {ElapsedMs}ms",
                elapsedMs);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("qdrant.initialize.errors");
            logger.LogError(ex, "Error initializing Qdrant collections");
            return Result<bool>.Failure($"Failed to initialize collections: {ex.Message}");
        }
    }

    /// <summary>
    /// Ensures a collection exists, creating it if necessary.
    /// </summary>
    private async Task EnsureCollectionExistsAsync(
        QdrantClient client,
        string collectionName,
        int vectorSize,
        CancellationToken cancellationToken)
    {
        try
        {
            // Check if collection exists
            var collections = await client.ListCollectionsAsync(cancellationToken);
            var exists = collections.Any(c => c == collectionName);

            if (exists)
            {
                logger.LogDebug("Collection {CollectionName} already exists", collectionName);
                return;
            }

            // Create collection with HNSW index for fast similarity search
            await client.CreateCollectionAsync(
                collectionName: collectionName,
                vectorsConfig: new VectorParams
                {
                    Size = (ulong)vectorSize,
                    Distance = Distance.Cosine, // Cosine similarity
                },
                cancellationToken: cancellationToken);

            logger.LogInformation(
                "Created Qdrant collection: {CollectionName} (vector size: {VectorSize})",
                collectionName, vectorSize);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error ensuring collection {CollectionName} exists", collectionName);
            throw;
        }
    }

    /// <summary>
    /// Checks if a collection exists.
    /// </summary>
    public async Task<Result<bool>> CollectionExistsAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = GetClient();
            var collections = await client.ListCollectionsAsync(cancellationToken);
            var exists = collections.Any(c => c == collectionName);

            return Result<bool>.Success(exists);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error checking if collection {CollectionName} exists", collectionName);
            return Result<bool>.Failure($"Failed to check collection: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the count of vectors in a collection.
    /// </summary>
    public async Task<Result<long>> GetVectorCountAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = GetClient();
            var collectionInfo = await client.GetCollectionInfoAsync(
                collectionName, cancellationToken);

            var count = (long)collectionInfo.PointsCount;
            return Result<long>.Success(count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting vector count for {CollectionName}", collectionName);
            return Result<long>.Failure($"Failed to get vector count: {ex.Message}");
        }
    }

    /// <summary>
    /// Upserts a single vector point into a collection.
    /// </summary>
    public async Task<Result<bool>> UpsertVectorAsync(
        string collectionName,
        ulong pointId,
        float[] vector,
        Dictionary<string, object> payload,
        CancellationToken cancellationToken = default)
    {
        metrics.IncrementCounter("qdrant.upsert.requests");
        var startTime = DateTime.UtcNow;

        try
        {
            var client = GetClient();

            var qdrantPayload = ConvertToQdrantPayload(payload);

            var point = new PointStruct
            {
                Id = new PointId { Num = pointId },
                Vectors = vector,
                Payload = { qdrantPayload }
            };

            await client.UpsertAsync(
                collectionName: collectionName,
                points: [point],
                cancellationToken: cancellationToken);

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("qdrant.upsert", elapsedMs);

            logger.LogDebug(
                "Upserted vector {PointId} to {CollectionName} in {ElapsedMs}ms",
                pointId, collectionName, elapsedMs);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("qdrant.upsert.errors");
            logger.LogError(ex, "Error upserting vector {PointId} to {CollectionName}",
                pointId, collectionName);
            return Result<bool>.Failure($"Failed to upsert vector: {ex.Message}");
        }
    }

    /// <summary>
    /// Upserts multiple vector points into a collection in batch.
    /// </summary>
    public async Task<Result<bool>> UpsertVectorsBatchAsync(
        string collectionName,
        List<ulong> pointIds,
        List<float[]> vectors,
        List<Dictionary<string, object>> payloads,
        CancellationToken cancellationToken = default)
    {
        if (pointIds.Count != vectors.Count || pointIds.Count != payloads.Count)
        {
            return Result<bool>.Failure(
                "Point IDs, vectors, and payloads must have the same count");
        }

        metrics.IncrementCounter("qdrant.upsert-batch.requests");
        var startTime = DateTime.UtcNow;

        try
        {
            var client = GetClient();

            var points = pointIds.Select((id, index) =>
            {
                var qdrantPayload = ConvertToQdrantPayload(payloads[index]);
                return new PointStruct
                {
                    Id = new PointId { Num = id },
                    Vectors = vectors[index],
                    Payload = { qdrantPayload }
                };
            }).ToList();

            await client.UpsertAsync(
                collectionName: collectionName,
                points: points,
                cancellationToken: cancellationToken);

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("qdrant.upsert-batch", elapsedMs);

            logger.LogInformation(
                "Batch upserted {Count} vectors to {CollectionName} in {ElapsedMs}ms",
                pointIds.Count, collectionName, elapsedMs);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("qdrant.upsert-batch.errors");
            logger.LogError(ex, "Error batch upserting {Count} vectors to {CollectionName}",
                pointIds.Count, collectionName);
            return Result<bool>.Failure($"Failed to batch upsert vectors: {ex.Message}");
        }
    }

    /// <summary>
    /// Searches for similar vectors in a collection.
    /// </summary>
    public async Task<Result<List<VectorSearchResult>>> SearchSimilarAsync(
        string collectionName,
        float[] queryVector,
        int limit = 5,
        float scoreThreshold = 0.0f,
        Dictionary<string, object>? filter = null,
        CancellationToken cancellationToken = default)
    {
        metrics.IncrementCounter("qdrant.search.requests");
        var startTime = DateTime.UtcNow;

        try
        {
            var client = GetClient();

            var results = await client.SearchAsync(
                collectionName: collectionName,
                vector: queryVector,
                limit: (ulong)limit,
                scoreThreshold: scoreThreshold,
                payloadSelector: true, // Include payload in results
                cancellationToken: cancellationToken);

            var searchResults = results.Select(r => new VectorSearchResult
            {
                Id = r.Id.Num,
                Score = r.Score,
                Payload = r.Payload.ToDictionary(
                    kvp => kvp.Key,
                    kvp => ConvertPayloadValue(kvp.Value))
            }).ToList();

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("qdrant.search", elapsedMs);

            logger.LogDebug(
                "Searched {CollectionName}, found {Count} results in {ElapsedMs}ms",
                collectionName, searchResults.Count, elapsedMs);

            return Result<List<VectorSearchResult>>.Success(searchResults);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("qdrant.search.errors");
            logger.LogError(ex, "Error searching {CollectionName}", collectionName);
            return Result<List<VectorSearchResult>>.Failure(
                $"Failed to search vectors: {ex.Message}");
        }
    }

    /// <summary>
    /// Converts a standard dictionary to Qdrant payload format.
    /// </summary>
    private static Dictionary<string, Value> ConvertToQdrantPayload(Dictionary<string, object> payload)
    {
        var qdrantPayload = new Dictionary<string, Value>();

        foreach (var kvp in payload)
        {
            qdrantPayload[kvp.Key] = ConvertToQdrantValue(kvp.Value);
        }

        return qdrantPayload;
    }

    /// <summary>
    /// Converts an object to Qdrant Value format.
    /// </summary>
    private static Value ConvertToQdrantValue(object value)
    {
        return value switch
        {
            null => new Value { NullValue = Qdrant.Client.Grpc.NullValue.NullValue },
            bool boolValue => new Value { BoolValue = boolValue },
            int intValue => new Value { IntegerValue = intValue },
            long longValue => new Value { IntegerValue = longValue },
            float floatValue => new Value { DoubleValue = floatValue },
            double doubleValue => new Value { DoubleValue = doubleValue },
            string stringValue => new Value { StringValue = stringValue },
            _ => new Value { StringValue = value.ToString() ?? string.Empty }
        };
    }

    /// <summary>
    /// Converts Qdrant payload value to a standard object.
    /// </summary>
    private static object ConvertPayloadValue(Value value)
    {
        return value.KindCase switch
        {
            Value.KindOneofCase.IntegerValue => value.IntegerValue,
            Value.KindOneofCase.DoubleValue => value.DoubleValue,
            Value.KindOneofCase.StringValue => value.StringValue,
            Value.KindOneofCase.BoolValue => value.BoolValue,
            Value.KindOneofCase.ListValue => value.ListValue.Values
                .Select(ConvertPayloadValue).ToList(),
            Value.KindOneofCase.StructValue => value.StructValue.Fields
                .ToDictionary(kvp => kvp.Key, kvp => ConvertPayloadValue(kvp.Value)),
            _ => value.ToString()
        };
    }

    /// <summary>
    /// Deletes a vector point from a collection by ID.
    /// </summary>
    public async Task<Result<bool>> DeleteVectorAsync(
        string collectionName,
        ulong pointId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = GetClient();

            await client.DeleteAsync(
                collectionName: collectionName,
                ids: [new PointId { Num = pointId }],
                cancellationToken: cancellationToken);

            logger.LogDebug("Deleted vector {PointId} from {CollectionName}",
                pointId, collectionName);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting vector {PointId} from {CollectionName}",
                pointId, collectionName);
            return Result<bool>.Failure($"Failed to delete vector: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes all vectors from a collection.
    /// </summary>
    public async Task<Result<bool>> ClearCollectionAsync(
        string collectionName,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = GetClient();

            // Delete collection and recreate it
            await client.DeleteCollectionAsync(collectionName);

            logger.LogInformation("Cleared collection {CollectionName}", collectionName);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error clearing collection {CollectionName}", collectionName);
            return Result<bool>.Failure($"Failed to clear collection: {ex.Message}");
        }
    }

    /// <summary>
    /// Tests connection to Qdrant service.
    /// </summary>
    public async Task<Result<bool>> TestConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var client = GetClient();
            var collections = await client.ListCollectionsAsync(cancellationToken);

            logger.LogInformation(
                "Qdrant connection test successful ({Count} collections)",
                collections.Count);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Qdrant connection test failed");
            return Result<bool>.Failure($"Connection test failed: {ex.Message}");
        }
    }
}
