using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NetSuiteRAG.Api.Data;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;
using System.Text.Json;

namespace NetSuiteRAG.Api.Services.Implementations;

/// <summary>
/// Service for managing and retrieving NetSuite field definitions with Redis caching.
/// Implements a cache-aside pattern with 24-hour TTL for standard fields.
/// </summary>
public class FieldDictionaryService(
    AppDbContext dbContext,
    IDistributedCache cache,
    ILogger<FieldDictionaryService> logger) : IFieldDictionaryService
{
    private const int CacheTtlHours = 24;
    private const string CacheKeyPrefix = "field";

    /// <inheritdoc/>
    public async Task<FieldDefinition?> GetFieldAsync(
        string recordType,
        string fieldId,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordType);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);

        var cacheKey = GetCacheKey(recordType, fieldId);

        // Try cache first
        var cachedJson = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            logger.LogDebug(
                "Cache hit for field {FieldId} in {RecordType}",
                fieldId,
                recordType);

            return JsonSerializer.Deserialize<FieldDefinition>(cachedJson);
        }

        logger.LogDebug(
            "Cache miss for field {FieldId} in {RecordType}, querying database",
            fieldId,
            recordType);

        // Query database
        var field = await dbContext.FieldDefinitions
            .AsNoTracking()
            .FirstOrDefaultAsync(
                f => f.RecordType == recordType && f.FieldId == fieldId,
                cancellationToken);

        if (field != null)
        {
            // Cache the result
            await CacheFieldAsync(cacheKey, field, cancellationToken);
        }

        return field;
    }

    /// <inheritdoc/>
    public async Task<List<FieldDefinition>> GetFieldsForRecordTypeAsync(
        string recordType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordType);

        var cacheKey = GetCacheKey(recordType, "all");

        // Try cache first
        var cachedJson = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            logger.LogDebug(
                "Cache hit for all fields in {RecordType}",
                recordType);

            return JsonSerializer.Deserialize<List<FieldDefinition>>(cachedJson)
                ?? [];
        }

        logger.LogDebug(
            "Cache miss for all fields in {RecordType}, querying database",
            recordType);

        // Query database
        var fields = await dbContext.FieldDefinitions
            .AsNoTracking()
            .Where(f => f.RecordType == recordType)
            .OrderBy(f => f.Label)
            .ToListAsync(cancellationToken);

        if (fields.Count > 0)
        {
            // Cache the result
            var json = JsonSerializer.Serialize(fields);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTtlHours)
            };
            await cache.SetStringAsync(cacheKey, json, options, cancellationToken);
        }

        logger.LogInformation(
            "Retrieved {FieldCount} fields for {RecordType}",
            fields.Count,
            recordType);

        return fields;
    }

    /// <inheritdoc/>
    public async Task<List<FieldDefinition>> SearchFieldsAsync(
        string recordType,
        string searchQuery,
        int maxResults = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordType);
        ArgumentException.ThrowIfNullOrWhiteSpace(searchQuery);

        if (maxResults <= 0)
        {
            throw new ArgumentException("Max results must be greater than 0", nameof(maxResults));
        }

        logger.LogDebug(
            "Searching fields in {RecordType} for query '{SearchQuery}'",
            recordType,
            searchQuery);

        var query = searchQuery.ToLowerInvariant();

        // Search by label, field ID, and aliases
        var fields = await dbContext.FieldDefinitions
            .AsNoTracking()
            .Where(f => f.RecordType == recordType)
            .Where(f =>
                f.Label.ToLower().Contains(query) ||
                f.FieldId.ToLower().Contains(query) ||
                (f.Aliases != null && f.Aliases.Any(a => a.ToLower().Contains(query))))
            .Take(maxResults)
            .ToListAsync(cancellationToken);

        // Sort by relevance (exact matches first, then starts with, then contains)
        var sortedFields = fields
            .OrderByDescending(f =>
            {
                var label = f.Label.ToLowerInvariant();
                var fieldId = f.FieldId.ToLowerInvariant();

                if (label == query || fieldId == query) return 3; // Exact match
                if (label.StartsWith(query) || fieldId.StartsWith(query)) return 2; // Starts with
                return 1; // Contains
            })
            .ThenBy(f => f.Label)
            .ToList();

        logger.LogInformation(
            "Found {ResultCount} fields matching '{SearchQuery}' in {RecordType}",
            sortedFields.Count,
            searchQuery,
            recordType);

        return sortedFields;
    }

    /// <inheritdoc/>
    public async Task<List<FieldDefinition>> GetFieldsForJoinAsync(
        string recordType,
        string joinName,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordType);
        ArgumentException.ThrowIfNullOrWhiteSpace(joinName);

        var cacheKey = GetCacheKey(recordType, $"join:{joinName}");

        // Try cache first
        var cachedJson = await cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedJson))
        {
            logger.LogDebug(
                "Cache hit for join {JoinName} in {RecordType}",
                joinName,
                recordType);

            return JsonSerializer.Deserialize<List<FieldDefinition>>(cachedJson)
                ?? [];
        }

        logger.LogDebug(
            "Cache miss for join {JoinName} in {RecordType}, querying database",
            joinName,
            recordType);

        // Query database
        var fields = await dbContext.FieldDefinitions
            .AsNoTracking()
            .Where(f => f.RecordType == recordType && f.RequiredJoin == joinName)
            .OrderBy(f => f.Label)
            .ToListAsync(cancellationToken);

        if (fields.Count > 0)
        {
            // Cache the result
            var json = JsonSerializer.Serialize(fields);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTtlHours)
            };
            await cache.SetStringAsync(cacheKey, json, options, cancellationToken);
        }

        logger.LogInformation(
            "Retrieved {FieldCount} fields for join {JoinName} in {RecordType}",
            fields.Count,
            joinName,
            recordType);

        return fields;
    }

    /// <inheritdoc/>
    public async Task<(bool IsValid, string? ErrorMessage)> ValidateFieldOperatorAsync(
        string recordType,
        string fieldId,
        OperatorType operatorType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordType);
        ArgumentException.ThrowIfNullOrWhiteSpace(fieldId);

        var field = await GetFieldAsync(recordType, fieldId, cancellationToken);

        if (field == null)
        {
            return (false, $"Field '{fieldId}' not found in record type '{recordType}'");
        }

        if (!field.ValidOperators.Contains(operatorType))
        {
            var validOps = string.Join(", ", field.ValidOperators.Select(o => o.ToString()));
            return (false,
                $"Operator '{operatorType}' is not valid for field '{fieldId}' " +
                $"of type '{field.FieldType}'. Valid operators: {validOps}");
        }

        return (true, null);
    }

    /// <inheritdoc/>
    public async Task InvalidateCacheAsync(
        string recordType,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(recordType);

        logger.LogInformation(
            "Invalidating cache for record type {RecordType}",
            recordType);

        // Note: This is a simplified implementation
        // In production, you might want to track all cache keys for a record type
        // or use Redis pattern matching to delete multiple keys
        var allFieldsKey = GetCacheKey(recordType, "all");
        await cache.RemoveAsync(allFieldsKey, cancellationToken);

        logger.LogInformation(
            "Cache invalidated for record type {RecordType}",
            recordType);
    }

    /// <summary>
    /// Generates a cache key for field lookups.
    /// </summary>
    /// <param name="recordType">Record type.</param>
    /// <param name="suffix">Key suffix (field ID or special identifier).</param>
    /// <returns>Cache key.</returns>
    private static string GetCacheKey(string recordType, string suffix)
    {
        return $"{CacheKeyPrefix}:{recordType}:{suffix}";
    }

    /// <summary>
    /// Caches a field definition in Redis.
    /// </summary>
    /// <param name="cacheKey">Cache key.</param>
    /// <param name="field">Field definition to cache.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    private async Task CacheFieldAsync(
        string cacheKey,
        FieldDefinition field,
        CancellationToken cancellationToken)
    {
        try
        {
            var json = JsonSerializer.Serialize(field);
            var options = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(CacheTtlHours)
            };
            await cache.SetStringAsync(cacheKey, json, options, cancellationToken);

            logger.LogDebug(
                "Cached field {FieldId} in {RecordType} with TTL {TtlHours}h",
                field.FieldId,
                field.RecordType,
                CacheTtlHours);
        }
        catch (Exception ex)
        {
            // Don't fail the request if caching fails
            logger.LogWarning(
                ex,
                "Failed to cache field {FieldId} in {RecordType}",
                field.FieldId,
                field.RecordType);
        }
    }
}
