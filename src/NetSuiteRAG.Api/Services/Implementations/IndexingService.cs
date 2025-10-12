using Microsoft.EntityFrameworkCore;
using NetSuiteRAG.Api.Data;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Api.Utilities;
using NetSuiteRAG.Shared.Models;
using NetSuiteRAG.Shared.Monitoring;

namespace NetSuiteRAG.Api.Services.Implementations;

/// <summary>
/// Service for indexing artifacts to the vector database.
/// </summary>
public class IndexingService(
    AppDbContext dbContext,
    IVectorStoreService vectorStore,
    IOllamaEmbeddingService embeddingService,
    MetricsCollector metrics,
    ILogger<IndexingService> logger) : IIndexingService
{
    private const string FieldDescriptorsCollection = "field_descriptors";
    private const string GlossaryTermsCollection = "glossary_terms";
    private const string QueryExemplarsCollection = "query_exemplars";

    /// <summary>
    /// Indexes all field descriptors to the vector database.
    /// </summary>
    public async Task<Result<int>> IndexFieldDescriptorsAsync(
        CancellationToken cancellationToken = default)
    {
        metrics.IncrementCounter("indexing.field-descriptors.requests");
        var startTime = DateTime.UtcNow;

        try
        {
            // Initialize collection if needed
            var initResult = await vectorStore.InitializeCollectionsAsync(
                cancellationToken: cancellationToken);

            if (!initResult.IsSuccess)
            {
                return Result<int>.Failure(
                    $"Failed to initialize collections: {initResult.Error}");
            }

            // Get all active and enriched field descriptors
            var fieldDescriptors = await dbContext.CustomFieldDescriptors
                .Where(fd => fd.EnrichedAt != null && !fd.IsStale)
                .ToListAsync(cancellationToken);

            if (fieldDescriptors.Count == 0)
            {
                logger.LogWarning("No enriched field descriptors found to index");
                return Result<int>.Success(0);
            }

            logger.LogInformation(
                "Indexing {Count} field descriptors to Qdrant",
                fieldDescriptors.Count);

            // Get existing point IDs from Qdrant to avoid duplicates
            var existingIds = await QdrantHelpers.GetExistingPointIdsAsync(
                vectorStore, FieldDescriptorsCollection, logger, cancellationToken);

            // Filter out already-indexed field descriptors
            var toIndex = QdrantHelpers.FilterUnindexedItems(
                fieldDescriptors, existingIds, fd => fd.Id);

            if (toIndex.Count == 0)
            {
                logger.LogInformation("All field descriptors are already indexed");
                return Result<int>.Success(0);
            }

            logger.LogInformation(
                "Indexing {Count} new field descriptors ({Total} total in DB, {Existing} already indexed)",
                toIndex.Count, fieldDescriptors.Count, existingIds.Count);

            // Generate combined text for embeddings and index in batches
            var indexed = 0;
            const int batchSize = 50;

            for (int i = 0; i < toIndex.Count; i += batchSize)
            {
                var batch = toIndex.Skip(i).Take(batchSize).ToList();

                // Generate combined text for each field descriptor
                var texts = batch.Select(fd => GenerateFieldDescriptorText(fd)).ToList();

                // Generate embeddings in batch
                var embeddingResult = await embeddingService.GenerateBatchEmbeddingsAsync(
                    texts, cancellationToken);

                if (!embeddingResult.IsSuccess)
                {
                    metrics.IncrementCounter("indexing.field-descriptors.errors");
                    logger.LogError(
                        "Failed to generate embeddings for batch starting at index {Index}: {Error}",
                        i, embeddingResult.Error);
                    continue;
                }

                var embeddings = embeddingResult.Value!;

                // Prepare batch for upserting
                var pointIds = QdrantHelpers.ConvertGuidsToPointIds(batch.Select(fd => fd.Id));
                var payloads = batch.Select(fd => new Dictionary<string, object>
                {
                    ["id"] = fd.Id.ToString(),
                    ["fieldId"] = fd.FieldId,
                    ["recordType"] = fd.RecordType,
                    ["label"] = fd.Label,
                    ["type"] = fd.Type,
                    ["aliases"] = fd.Aliases ?? new List<string>(),
                    ["businessContext"] = fd.BusinessContext ?? string.Empty,
                    ["ambiguityScore"] = (double)(fd.AmbiguityScore ?? 0),
                    ["enrichedAt"] = fd.EnrichedAt!.Value.ToString("O")
                }).ToList();

                // Upsert batch to Qdrant
                var upsertResult = await vectorStore.UpsertVectorsBatchAsync(
                    FieldDescriptorsCollection,
                    pointIds,
                    embeddings,
                    payloads,
                    cancellationToken);

                if (!upsertResult.IsSuccess)
                {
                    metrics.IncrementCounter("indexing.field-descriptors.errors");
                    logger.LogError(
                        "Failed to upsert batch starting at index {Index}: {Error}",
                        i, upsertResult.Error);
                    continue;
                }

                indexed += batch.Count;
                logger.LogDebug("Indexed {Count}/{Total} field descriptors",
                    indexed, toIndex.Count);
            }

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("indexing.field-descriptors", elapsedMs);

            logger.LogInformation(
                "Indexed {Count} field descriptors in {ElapsedMs}ms",
                indexed, elapsedMs);

            return Result<int>.Success(indexed);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("indexing.field-descriptors.errors");
            logger.LogError(ex, "Error indexing field descriptors");
            return Result<int>.Failure($"Indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Indexes all glossary terms to the vector database.
    /// </summary>
    public async Task<Result<int>> IndexGlossaryTermsAsync(
        CancellationToken cancellationToken = default)
    {
        metrics.IncrementCounter("indexing.glossary-terms.requests");
        var startTime = DateTime.UtcNow;

        try
        {
            // Initialize collection if needed
            var initResult = await vectorStore.InitializeCollectionsAsync(
                cancellationToken: cancellationToken);

            if (!initResult.IsSuccess)
            {
                return Result<int>.Failure(
                    $"Failed to initialize collections: {initResult.Error}");
            }

            // Get all active glossary terms
            var glossaryTerms = await dbContext.GlossaryTerms
                .Where(gt => gt.IsActive)
                .ToListAsync(cancellationToken);

            if (glossaryTerms.Count == 0)
            {
                logger.LogWarning("No active glossary terms found to index");
                return Result<int>.Success(0);
            }

            logger.LogInformation(
                "Indexing {Count} glossary terms to Qdrant",
                glossaryTerms.Count);

            // Get existing point IDs from Qdrant to avoid duplicates
            var existingIds = await QdrantHelpers.GetExistingPointIdsAsync(
                vectorStore, GlossaryTermsCollection, logger, cancellationToken);

            // Filter out already-indexed glossary terms
            var toIndex = QdrantHelpers.FilterUnindexedItems(
                glossaryTerms, existingIds, gt => gt.Id);

            if (toIndex.Count == 0)
            {
                logger.LogInformation("All glossary terms are already indexed");
                return Result<int>.Success(0);
            }

            logger.LogInformation(
                "Indexing {Count} new glossary terms ({Total} total in DB, {Existing} already indexed)",
                toIndex.Count, glossaryTerms.Count, existingIds.Count);

            // Generate combined text for embeddings and index in batches
            var indexed = 0;
            const int batchSize = 50;

            for (int i = 0; i < toIndex.Count; i += batchSize)
            {
                var batch = toIndex.Skip(i).Take(batchSize).ToList();

                // Generate combined text for each glossary term
                var texts = batch.Select(gt => GenerateGlossaryTermText(gt)).ToList();

                // Generate embeddings in batch
                var embeddingResult = await embeddingService.GenerateBatchEmbeddingsAsync(
                    texts, cancellationToken);

                if (!embeddingResult.IsSuccess)
                {
                    metrics.IncrementCounter("indexing.glossary-terms.errors");
                    logger.LogError(
                        "Failed to generate embeddings for batch starting at index {Index}: {Error}",
                        i, embeddingResult.Error);
                    continue;
                }

                var embeddings = embeddingResult.Value!;

                // Prepare batch for upserting
                var pointIds = QdrantHelpers.ConvertGuidsToPointIds(batch.Select(gt => gt.Id));
                var payloads = batch.Select(gt => new Dictionary<string, object>
                {
                    ["id"] = gt.Id.ToString(),
                    ["term"] = gt.Term,
                    ["definition"] = gt.Definition,
                    ["synonyms"] = gt.Synonyms,
                    ["category"] = gt.Category,
                    ["examples"] = gt.Examples,
                    ["createdAt"] = gt.CreatedAt.ToString("O")
                }).ToList();

                // Upsert batch to Qdrant
                var upsertResult = await vectorStore.UpsertVectorsBatchAsync(
                    GlossaryTermsCollection,
                    pointIds,
                    embeddings,
                    payloads,
                    cancellationToken);

                if (!upsertResult.IsSuccess)
                {
                    metrics.IncrementCounter("indexing.glossary-terms.errors");
                    logger.LogError(
                        "Failed to upsert batch starting at index {Index}: {Error}",
                        i, upsertResult.Error);
                    continue;
                }

                indexed += batch.Count;
                logger.LogDebug("Indexed {Count}/{Total} glossary terms",
                    indexed, toIndex.Count);
            }

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("indexing.glossary-terms", elapsedMs);

            logger.LogInformation(
                "Indexed {Count} glossary terms in {ElapsedMs}ms",
                indexed, elapsedMs);

            return Result<int>.Success(indexed);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("indexing.glossary-terms.errors");
            logger.LogError(ex, "Error indexing glossary terms");
            return Result<int>.Failure($"Indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Indexes all query exemplars to the vector database.
    /// </summary>
    public async Task<Result<int>> IndexQueryExemplarsAsync(
        CancellationToken cancellationToken = default)
    {
        metrics.IncrementCounter("indexing.query-exemplars.requests");
        var startTime = DateTime.UtcNow;

        try
        {
            // Initialize collection if needed
            var initResult = await vectorStore.InitializeCollectionsAsync(
                cancellationToken: cancellationToken);

            if (!initResult.IsSuccess)
            {
                return Result<int>.Failure(
                    $"Failed to initialize collections: {initResult.Error}");
            }

            // Get all active query exemplars
            var queryExemplars = await dbContext.QueryExemplars
                .Where(qe => qe.IsActive)
                .ToListAsync(cancellationToken);

            if (queryExemplars.Count == 0)
            {
                logger.LogWarning("No active query exemplars found to index");
                return Result<int>.Success(0);
            }

            logger.LogInformation(
                "Indexing {Count} query exemplars to Qdrant",
                queryExemplars.Count);

            // Get existing point IDs from Qdrant to avoid duplicates
            var existingIds = await QdrantHelpers.GetExistingPointIdsAsync(
                vectorStore, QueryExemplarsCollection, logger, cancellationToken);

            // Filter out already-indexed query exemplars
            var toIndex = QdrantHelpers.FilterUnindexedItems(
                queryExemplars, existingIds, qe => qe.Id);

            if (toIndex.Count == 0)
            {
                logger.LogInformation("All query exemplars are already indexed");
                return Result<int>.Success(0);
            }

            logger.LogInformation(
                "Indexing {Count} new query exemplars ({Total} total in DB, {Existing} already indexed)",
                toIndex.Count, queryExemplars.Count, existingIds.Count);

            // Generate combined text for embeddings and index in batches
            var indexed = 0;
            const int batchSize = 50;

            for (int i = 0; i < toIndex.Count; i += batchSize)
            {
                var batch = toIndex.Skip(i).Take(batchSize).ToList();

                // Generate combined text for each query exemplar
                var texts = batch.Select(qe => GenerateQueryExemplarText(qe)).ToList();

                // Generate embeddings in batch
                var embeddingResult = await embeddingService.GenerateBatchEmbeddingsAsync(
                    texts, cancellationToken);

                if (!embeddingResult.IsSuccess)
                {
                    metrics.IncrementCounter("indexing.query-exemplars.errors");
                    logger.LogError(
                        "Failed to generate embeddings for batch starting at index {Index}: {Error}",
                        i, embeddingResult.Error);
                    continue;
                }

                var embeddings = embeddingResult.Value!;

                // Prepare batch for upserting
                var pointIds = QdrantHelpers.ConvertGuidsToPointIds(batch.Select(qe => qe.Id));
                var payloads = batch.Select(qe => new Dictionary<string, object>
                {
                    ["id"] = qe.Id.ToString(),
                    ["naturalQuery"] = qe.NaturalQuery,
                    ["savedSearchPlan"] = qe.SavedSearchPlanJson,
                    ["explanation"] = qe.Explanation,
                    ["tags"] = qe.Tags,
                    ["difficulty"] = qe.Difficulty.ToString(),
                    ["recordType"] = qe.RecordType,
                    ["createdAt"] = qe.CreatedAt.ToString("O")
                }).ToList();

                // Upsert batch to Qdrant
                var upsertResult = await vectorStore.UpsertVectorsBatchAsync(
                    QueryExemplarsCollection,
                    pointIds,
                    embeddings,
                    payloads,
                    cancellationToken);

                if (!upsertResult.IsSuccess)
                {
                    metrics.IncrementCounter("indexing.query-exemplars.errors");
                    logger.LogError(
                        "Failed to upsert batch starting at index {Index}: {Error}",
                        i, upsertResult.Error);
                    continue;
                }

                indexed += batch.Count;
                logger.LogDebug("Indexed {Count}/{Total} query exemplars",
                    indexed, toIndex.Count);
            }

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("indexing.query-exemplars", elapsedMs);

            logger.LogInformation(
                "Indexed {Count} query exemplars in {ElapsedMs}ms",
                indexed, elapsedMs);

            return Result<int>.Success(indexed);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("indexing.query-exemplars.errors");
            logger.LogError(ex, "Error indexing query exemplars");
            return Result<int>.Failure($"Indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Indexes all artifacts to the vector database.
    /// </summary>
    public async Task<Result<IndexingStatistics>> IndexAllAsync(
        CancellationToken cancellationToken = default)
    {
        metrics.IncrementCounter("indexing.all.requests");
        var startTime = DateTime.UtcNow;

        try
        {
            logger.LogInformation("Starting full indexing of all artifacts");

            // Index field descriptors
            var fieldResult = await IndexFieldDescriptorsAsync(cancellationToken);
            var fieldCount = fieldResult.IsSuccess ? fieldResult.Value : 0;

            // Index glossary terms
            var glossaryResult = await IndexGlossaryTermsAsync(cancellationToken);
            var glossaryCount = glossaryResult.IsSuccess ? glossaryResult.Value : 0;

            // Index query exemplars
            var exemplarResult = await IndexQueryExemplarsAsync(cancellationToken);
            var exemplarCount = exemplarResult.IsSuccess ? exemplarResult.Value : 0;

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("indexing.all", elapsedMs);

            var statistics = new IndexingStatistics
            {
                FieldDescriptorsIndexed = fieldCount,
                GlossaryTermsIndexed = glossaryCount,
                QueryExemplarsIndexed = exemplarCount,
                ElapsedMs = elapsedMs
            };

            logger.LogInformation(
                "Completed full indexing: {Total} total artifacts indexed in {ElapsedMs}ms " +
                "({FieldCount} field descriptors, {GlossaryCount} glossary terms, {ExemplarCount} query exemplars)",
                statistics.TotalIndexed, elapsedMs, fieldCount, glossaryCount, exemplarCount);

            return Result<IndexingStatistics>.Success(statistics);
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("indexing.all.errors");
            logger.LogError(ex, "Error during full indexing");
            return Result<IndexingStatistics>.Failure($"Full indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the current indexing status for all collections.
    /// </summary>
    public async Task<Result<IndexingStatus>> GetIndexingStatusAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if collections exist
            var fieldCollectionExists = await vectorStore.CollectionExistsAsync(
                FieldDescriptorsCollection, cancellationToken);
            var glossaryCollectionExists = await vectorStore.CollectionExistsAsync(
                GlossaryTermsCollection, cancellationToken);
            var exemplarCollectionExists = await vectorStore.CollectionExistsAsync(
                QueryExemplarsCollection, cancellationToken);

            var collectionsInitialized =
                fieldCollectionExists.IsSuccess && fieldCollectionExists.Value &&
                glossaryCollectionExists.IsSuccess && glossaryCollectionExists.Value &&
                exemplarCollectionExists.IsSuccess && exemplarCollectionExists.Value;

            // Get database counts
            var fieldDescriptorsInDb = await dbContext.CustomFieldDescriptors
                .Where(fd => fd.EnrichedAt != null && !fd.IsStale)
                .LongCountAsync(cancellationToken);

            var glossaryTermsInDb = await dbContext.GlossaryTerms
                .Where(gt => gt.IsActive)
                .LongCountAsync(cancellationToken);

            var queryExemplarsInDb = await dbContext.QueryExemplars
                .Where(qe => qe.IsActive)
                .LongCountAsync(cancellationToken);

            // Get vector store counts
            long fieldDescriptorsInVector = 0;
            long glossaryTermsInVector = 0;
            long queryExemplarsInVector = 0;

            if (collectionsInitialized)
            {
                var fieldCountResult = await vectorStore.GetVectorCountAsync(
                    FieldDescriptorsCollection, cancellationToken);
                fieldDescriptorsInVector = fieldCountResult.IsSuccess ? fieldCountResult.Value : 0;

                var glossaryCountResult = await vectorStore.GetVectorCountAsync(
                    GlossaryTermsCollection, cancellationToken);
                glossaryTermsInVector = glossaryCountResult.IsSuccess ? glossaryCountResult.Value : 0;

                var exemplarCountResult = await vectorStore.GetVectorCountAsync(
                    QueryExemplarsCollection, cancellationToken);
                queryExemplarsInVector = exemplarCountResult.IsSuccess ? exemplarCountResult.Value : 0;
            }

            var status = new IndexingStatus
            {
                FieldDescriptorsInDb = fieldDescriptorsInDb,
                FieldDescriptorsInVector = fieldDescriptorsInVector,
                GlossaryTermsInDb = glossaryTermsInDb,
                GlossaryTermsInVector = glossaryTermsInVector,
                QueryExemplarsInDb = queryExemplarsInDb,
                QueryExemplarsInVector = queryExemplarsInVector,
                CollectionsInitialized = collectionsInitialized
            };

            return Result<IndexingStatus>.Success(status);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting indexing status");
            return Result<IndexingStatus>.Failure($"Failed to get status: {ex.Message}");
        }
    }

    /// <summary>
    /// Re-indexes a specific field descriptor by ID.
    /// </summary>
    public async Task<Result<bool>> ReindexFieldDescriptorAsync(
        Guid fieldDescriptorId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var fieldDescriptor = await dbContext.CustomFieldDescriptors
                .FirstOrDefaultAsync(fd => fd.Id == fieldDescriptorId, cancellationToken);

            if (fieldDescriptor == null)
            {
                return Result<bool>.Failure($"Field descriptor {fieldDescriptorId} not found");
            }

            if (fieldDescriptor.EnrichedAt == null)
            {
                return Result<bool>.Failure("Field descriptor has not been enriched yet");
            }

            // Generate embedding
            var text = GenerateFieldDescriptorText(fieldDescriptor);
            var embeddingResult = await embeddingService.GenerateEmbeddingAsync(text, cancellationToken);

            if (!embeddingResult.IsSuccess)
            {
                return Result<bool>.Failure($"Failed to generate embedding: {embeddingResult.Error}");
            }

            // Upsert to Qdrant
            var pointId = QdrantHelpers.ConvertGuidToPointId(fieldDescriptor.Id);
            var payload = new Dictionary<string, object>
            {
                ["id"] = fieldDescriptor.Id.ToString(),
                ["fieldId"] = fieldDescriptor.FieldId,
                ["recordType"] = fieldDescriptor.RecordType,
                ["label"] = fieldDescriptor.Label,
                ["type"] = fieldDescriptor.Type,
                ["aliases"] = fieldDescriptor.Aliases ?? new List<string>(),
                ["businessContext"] = fieldDescriptor.BusinessContext ?? string.Empty,
                ["ambiguityScore"] = (double)(fieldDescriptor.AmbiguityScore ?? 0),
                ["enrichedAt"] = fieldDescriptor.EnrichedAt.Value.ToString("O")
            };

            var upsertResult = await vectorStore.UpsertVectorAsync(
                FieldDescriptorsCollection,
                pointId,
                embeddingResult.Value!,
                payload,
                cancellationToken);

            return upsertResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error re-indexing field descriptor {Id}", fieldDescriptorId);
            return Result<bool>.Failure($"Re-indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Re-indexes a specific glossary term by ID.
    /// </summary>
    public async Task<Result<bool>> ReindexGlossaryTermAsync(
        Guid glossaryTermId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var glossaryTerm = await dbContext.GlossaryTerms
                .FirstOrDefaultAsync(gt => gt.Id == glossaryTermId, cancellationToken);

            if (glossaryTerm == null)
            {
                return Result<bool>.Failure($"Glossary term {glossaryTermId} not found");
            }

            // Generate embedding
            var text = GenerateGlossaryTermText(glossaryTerm);
            var embeddingResult = await embeddingService.GenerateEmbeddingAsync(text, cancellationToken);

            if (!embeddingResult.IsSuccess)
            {
                return Result<bool>.Failure($"Failed to generate embedding: {embeddingResult.Error}");
            }

            // Upsert to Qdrant
            var pointId = QdrantHelpers.ConvertGuidToPointId(glossaryTerm.Id);
            var payload = new Dictionary<string, object>
            {
                ["id"] = glossaryTerm.Id.ToString(),
                ["term"] = glossaryTerm.Term,
                ["definition"] = glossaryTerm.Definition,
                ["synonyms"] = glossaryTerm.Synonyms,
                ["category"] = glossaryTerm.Category,
                ["examples"] = glossaryTerm.Examples,
                ["createdAt"] = glossaryTerm.CreatedAt.ToString("O")
            };

            var upsertResult = await vectorStore.UpsertVectorAsync(
                GlossaryTermsCollection,
                pointId,
                embeddingResult.Value!,
                payload,
                cancellationToken);

            return upsertResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error re-indexing glossary term {Id}", glossaryTermId);
            return Result<bool>.Failure($"Re-indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Re-indexes a specific query exemplar by ID.
    /// </summary>
    public async Task<Result<bool>> ReindexQueryExemplarAsync(
        Guid queryExemplarId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var queryExemplar = await dbContext.QueryExemplars
                .FirstOrDefaultAsync(qe => qe.Id == queryExemplarId, cancellationToken);

            if (queryExemplar == null)
            {
                return Result<bool>.Failure($"Query exemplar {queryExemplarId} not found");
            }

            // Generate embedding
            var text = GenerateQueryExemplarText(queryExemplar);
            var embeddingResult = await embeddingService.GenerateEmbeddingAsync(text, cancellationToken);

            if (!embeddingResult.IsSuccess)
            {
                return Result<bool>.Failure($"Failed to generate embedding: {embeddingResult.Error}");
            }

            // Upsert to Qdrant
            var pointId = QdrantHelpers.ConvertGuidToPointId(queryExemplar.Id);
            var payload = new Dictionary<string, object>
            {
                ["id"] = queryExemplar.Id.ToString(),
                ["naturalQuery"] = queryExemplar.NaturalQuery,
                ["savedSearchPlan"] = queryExemplar.SavedSearchPlanJson,
                ["explanation"] = queryExemplar.Explanation,
                ["tags"] = queryExemplar.Tags,
                ["difficulty"] = queryExemplar.Difficulty.ToString(),
                ["recordType"] = queryExemplar.RecordType,
                ["createdAt"] = queryExemplar.CreatedAt.ToString("O")
            };

            var upsertResult = await vectorStore.UpsertVectorAsync(
                QueryExemplarsCollection,
                pointId,
                embeddingResult.Value!,
                payload,
                cancellationToken);

            return upsertResult;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error re-indexing query exemplar {Id}", queryExemplarId);
            return Result<bool>.Failure($"Re-indexing failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates combined text for field descriptor embedding.
    /// </summary>
    private static string GenerateFieldDescriptorText(CustomFieldDescriptor fd)
    {
        var parts = new List<string>
        {
            $"Field: {fd.Label}",
            $"ID: {fd.FieldId}",
            $"Record Type: {fd.RecordType}",
            $"Type: {fd.Type}"
        };

        if (fd.Aliases?.Count > 0)
        {
            parts.Add($"Aliases: {string.Join(", ", fd.Aliases)}");
        }

        if (!string.IsNullOrWhiteSpace(fd.BusinessContext))
        {
            parts.Add($"Context: {fd.BusinessContext}");
        }

        if (!string.IsNullOrWhiteSpace(fd.Description))
        {
            parts.Add($"Description: {fd.Description}");
        }

        return string.Join(". ", parts);
    }

    /// <summary>
    /// Generates combined text for glossary term embedding.
    /// </summary>
    private static string GenerateGlossaryTermText(GlossaryTerm gt)
    {
        var parts = new List<string>
        {
            $"Term: {gt.Term}",
            $"Definition: {gt.Definition}"
        };

        if (gt.Synonyms.Count > 0)
        {
            parts.Add($"Synonyms: {string.Join(", ", gt.Synonyms)}");
        }

        if (gt.Examples.Count > 0)
        {
            parts.Add($"Examples: {string.Join("; ", gt.Examples)}");
        }

        parts.Add($"Category: {gt.Category}");

        return string.Join(". ", parts);
    }

    /// <summary>
    /// Generates combined text for query exemplar embedding.
    /// </summary>
    private static string GenerateQueryExemplarText(QueryExemplar qe)
    {
        var parts = new List<string>
        {
            $"Query: {qe.NaturalQuery}",
            $"Explanation: {qe.Explanation}",
            $"Record Type: {qe.RecordType}",
            $"Difficulty: {qe.Difficulty}"
        };

        if (qe.Tags.Count > 0)
        {
            parts.Add($"Tags: {string.Join(", ", qe.Tags)}");
        }

        return string.Join(". ", parts);
    }
}
