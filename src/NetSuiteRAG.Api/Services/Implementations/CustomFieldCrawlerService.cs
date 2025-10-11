using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using NetSuiteRAG.Api.Data;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Implementations;

/// <summary>
/// Service for crawling and tracking NetSuite custom fields.
/// Discovers custom fields, tracks changes, and flags stale fields.
/// </summary>
public class CustomFieldCrawlerService(
    INetSuiteApiService netSuiteApiService,
    AppDbContext dbContext,
    IDistributedCache cache,
    ILogger<CustomFieldCrawlerService> logger) : ICustomFieldCrawlerService
{
    private const int StaleThresholdDays = 90;

    /// <summary>
    /// Crawls NetSuite for custom fields and updates the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with count of fields processed.</returns>
    public async Task<Result<CrawlResult>> CrawlCustomFieldsAsync(
        CancellationToken cancellationToken = default)
    {
        var crawlStartedAt = DateTime.UtcNow;
        var newCount = 0;
        var updatedCount = 0;
        var staleCount = 0;

        try
        {
            logger.LogInformation("Starting custom field crawl");

            // Fetch custom fields from NetSuite
            var fetchResult = await netSuiteApiService.GetAllCustomFieldsAsync(cancellationToken);
            if (!fetchResult.IsSuccess || fetchResult.Value == null)
            {
                logger.LogError("Failed to fetch custom fields from NetSuite: {Error}", fetchResult.Error);
                return Result<CrawlResult>.Failure(fetchResult.Error ?? "Failed to fetch custom fields");
            }

            var customFields = fetchResult.Value;
            logger.LogInformation("Fetched {Count} custom fields from NetSuite", customFields.Count);

            // Process each custom field
            foreach (var field in customFields)
            {
                // Skip if no record types specified
                if (field.RecordTypes == null || field.RecordTypes.Count == 0)
                {
                    logger.LogWarning("Skipping field {FieldId} with no record types", field.Id);
                    continue;
                }

                // Process each record type the field applies to
                foreach (var recordType in field.RecordTypes)
                {
                    var processResult = await ProcessCustomFieldAsync(field, recordType, cancellationToken);
                    switch (processResult)
                    {
                        case ProcessResult.New:
                            newCount++;
                            break;
                        case ProcessResult.Updated:
                            updatedCount++;
                            break;
                    }
                }
            }

            // Mark stale fields (not seen in this crawl)
            staleCount = await MarkStaleFieldsAsync(crawlStartedAt, cancellationToken);

            // Save all changes
            await dbContext.SaveChangesAsync(cancellationToken);

            // Invalidate cache
            await InvalidateCacheAsync(cancellationToken);

            var crawlCompletedAt = DateTime.UtcNow;

            logger.LogInformation(
                "Completed custom field crawl: {New} new, {Updated} updated, {Stale} stale",
                newCount,
                updatedCount,
                staleCount);

            var result = new CrawlResult(
                customFields.Count,
                newCount,
                updatedCount,
                staleCount,
                crawlStartedAt,
                crawlCompletedAt
            );

            return Result<CrawlResult>.Success(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during custom field crawl");
            return Result<CrawlResult>.Failure($"Crawl failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all custom field descriptors from the database.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of custom field descriptors.</returns>
    public async Task<Result<List<CustomFieldDescriptor>>> GetAllCustomFieldDescriptorsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var descriptors = await dbContext.CustomFieldDescriptors
                .OrderBy(d => d.RecordType)
                .ThenBy(d => d.FieldId)
                .ToListAsync(cancellationToken);

            return Result<List<CustomFieldDescriptor>>.Success(descriptors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching custom field descriptors");
            return Result<List<CustomFieldDescriptor>>.Failure($"Failed to fetch descriptors: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets custom field descriptors for a specific record type.
    /// </summary>
    /// <param name="recordType">Record type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of custom field descriptors.</returns>
    public async Task<Result<List<CustomFieldDescriptor>>> GetCustomFieldDescriptorsByRecordTypeAsync(
        string recordType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var descriptors = await dbContext.CustomFieldDescriptors
                .Where(d => d.RecordType == recordType)
                .OrderBy(d => d.FieldId)
                .ToListAsync(cancellationToken);

            return Result<List<CustomFieldDescriptor>>.Success(descriptors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching custom field descriptors for record type {RecordType}", recordType);
            return Result<List<CustomFieldDescriptor>>.Failure($"Failed to fetch descriptors: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the change log for custom fields.
    /// </summary>
    /// <param name="limit">Maximum number of entries to return.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of change log entries.</returns>
    public async Task<Result<List<CustomFieldChangeLog>>> GetChangeLogAsync(
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var changes = await dbContext.CustomFieldChangeLogs
                .OrderByDescending(c => c.DetectedAt)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return Result<List<CustomFieldChangeLog>>.Success(changes);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching change log");
            return Result<List<CustomFieldChangeLog>>.Failure($"Failed to fetch change log: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets stale custom fields (not seen in 90+ days).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of stale custom field descriptors.</returns>
    public async Task<Result<List<CustomFieldDescriptor>>> GetStaleFieldsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var staleFields = await dbContext.CustomFieldDescriptors
                .Where(d => d.IsStale)
                .OrderBy(d => d.LastSeenAt)
                .ToListAsync(cancellationToken);

            return Result<List<CustomFieldDescriptor>>.Success(staleFields);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error fetching stale fields");
            return Result<List<CustomFieldDescriptor>>.Failure($"Failed to fetch stale fields: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes a single custom field for a record type.
    /// </summary>
    private async Task<ProcessResult> ProcessCustomFieldAsync(
        NetSuiteCustomFieldMetadata field,
        string recordType,
        CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        // Check if field already exists
        var existing = await dbContext.CustomFieldDescriptors
            .FirstOrDefaultAsync(
                d => d.RecordType == recordType && d.FieldId == field.Id,
                cancellationToken);

        if (existing == null)
        {
            // New field - create descriptor
            var descriptor = new CustomFieldDescriptor
            {
                Id = Guid.NewGuid(),
                FieldId = field.Id,
                RecordType = recordType,
                Label = field.Label,
                Type = field.Type,
                Description = field.Description,
                IsMandatory = field.IsMandatory,
                DefaultValue = field.DefaultValue,
                IsStale = false,
                CreatedAt = now,
                UpdatedAt = now,
                LastSeenAt = now,
                MetadataJson = field.AdditionalProperties != null
                    ? JsonSerializer.Serialize(field.AdditionalProperties)
                    : null
            };

            dbContext.CustomFieldDescriptors.Add(descriptor);

            // Log change
            var changeLog = new CustomFieldChangeLog
            {
                Id = Guid.NewGuid(),
                CustomFieldDescriptorId = descriptor.Id,
                FieldId = field.Id,
                ChangeType = ChangeType.New,
                OldValue = null,
                NewValue = JsonSerializer.Serialize(new
                {
                    field.Id,
                    field.Label,
                    field.Type,
                    RecordType = recordType
                }),
                DetectedAt = now,
                ChangeDescription = $"New custom field discovered: {field.Label}"
            };

            dbContext.CustomFieldChangeLogs.Add(changeLog);

            logger.LogInformation(
                "New custom field: {FieldId} ({Label}) for {RecordType}",
                field.Id,
                field.Label,
                recordType);

            return ProcessResult.New;
        }
        else
        {
            // Existing field - check for changes
            var hasChanges = false;
            var oldValues = new Dictionary<string, object?>();
            var newValues = new Dictionary<string, object?>();

            if (existing.Label != field.Label)
            {
                oldValues["label"] = existing.Label;
                newValues["label"] = field.Label;
                existing.Label = field.Label;
                hasChanges = true;
            }

            if (existing.Type != field.Type)
            {
                oldValues["type"] = existing.Type;
                newValues["type"] = field.Type;
                existing.Type = field.Type;
                hasChanges = true;
            }

            if (existing.Description != field.Description)
            {
                oldValues["description"] = existing.Description;
                newValues["description"] = field.Description;
                existing.Description = field.Description;
                hasChanges = true;
            }

            if (existing.IsMandatory != field.IsMandatory)
            {
                oldValues["isMandatory"] = existing.IsMandatory;
                newValues["isMandatory"] = field.IsMandatory;
                existing.IsMandatory = field.IsMandatory;
                hasChanges = true;
            }

            if (existing.DefaultValue != field.DefaultValue)
            {
                oldValues["defaultValue"] = existing.DefaultValue;
                newValues["defaultValue"] = field.DefaultValue;
                existing.DefaultValue = field.DefaultValue;
                hasChanges = true;
            }

            // Update timestamps
            existing.UpdatedAt = now;
            existing.LastSeenAt = now;

            // Clear stale flag if it was set
            if (existing.IsStale)
            {
                existing.IsStale = false;
                hasChanges = true;
            }

            // Log changes if any
            if (hasChanges)
            {
                var changeLog = new CustomFieldChangeLog
                {
                    Id = Guid.NewGuid(),
                    CustomFieldDescriptorId = existing.Id,
                    FieldId = field.Id,
                    ChangeType = ChangeType.Updated,
                    OldValue = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null,
                    NewValue = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null,
                    DetectedAt = now,
                    ChangeDescription = $"Field metadata updated"
                };

                dbContext.CustomFieldChangeLogs.Add(changeLog);

                logger.LogInformation(
                    "Updated custom field: {FieldId} for {RecordType}",
                    field.Id,
                    recordType);

                return ProcessResult.Updated;
            }
            else
            {
                // No changes, just update LastSeenAt
                return ProcessResult.NoChange;
            }
        }
    }

    /// <summary>
    /// Marks fields as stale if not seen in the current crawl.
    /// </summary>
    private async Task<int> MarkStaleFieldsAsync(
        DateTime crawlStartedAt,
        CancellationToken cancellationToken)
    {
        var staleThreshold = DateTime.UtcNow.AddDays(-StaleThresholdDays);

        // Find fields not seen since the stale threshold
        var staleFields = await dbContext.CustomFieldDescriptors
            .Where(d => d.LastSeenAt < staleThreshold && !d.IsStale)
            .ToListAsync(cancellationToken);

        foreach (var field in staleFields)
        {
            field.IsStale = true;
            field.UpdatedAt = DateTime.UtcNow;

            // Log stale detection
            var changeLog = new CustomFieldChangeLog
            {
                Id = Guid.NewGuid(),
                CustomFieldDescriptorId = field.Id,
                FieldId = field.FieldId,
                ChangeType = ChangeType.Stale,
                OldValue = null,
                NewValue = null,
                DetectedAt = DateTime.UtcNow,
                ChangeDescription = $"Field not seen since {field.LastSeenAt:yyyy-MM-dd}"
            };

            dbContext.CustomFieldChangeLogs.Add(changeLog);

            logger.LogWarning(
                "Marked field as stale: {FieldId} ({RecordType}), last seen {LastSeen:yyyy-MM-dd}",
                field.FieldId,
                field.RecordType,
                field.LastSeenAt);
        }

        return staleFields.Count;
    }

    /// <summary>
    /// Invalidates the Redis cache for custom field descriptors.
    /// </summary>
    private async Task InvalidateCacheAsync(CancellationToken cancellationToken)
    {
        try
        {
            // Invalidate all custom field cache keys
            // In production, you might want to be more selective
            logger.LogInformation("Invalidating custom field cache");

            // For now, we don't have specific cache keys to invalidate
            // This will be implemented when we add caching to the field dictionary service
            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error invalidating cache");
        }
    }

    /// <summary>
    /// Result of processing a custom field.
    /// </summary>
    private enum ProcessResult
    {
        New,
        Updated,
        NoChange
    }
}
