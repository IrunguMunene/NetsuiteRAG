using Microsoft.EntityFrameworkCore;
using NetSuiteRAG.Api.Data;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;
using NetSuiteRAG.Shared.Monitoring;
using System.Text.Json;

namespace NetSuiteRAG.Api.Services.Implementations;

/// <summary>
/// Service for enriching field definitions with aliases, business context, and embeddings.
/// </summary>
public class FieldEnrichmentService(
    AppDbContext dbContext,
    IOllamaEmbeddingService embeddingService,
    MetricsCollector metrics,
    ILogger<FieldEnrichmentService> logger) : IFieldEnrichmentService
{
    /// <summary>
    /// Enriches all field definitions that haven't been enriched yet.
    /// </summary>
    public async Task<Result<EnrichmentResult>> EnrichAllFieldsAsync(
        CancellationToken cancellationToken = default)
    {
        var startTime = DateTime.UtcNow;
        metrics.IncrementCounter("enrichment.enrich-all.requests");

        try
        {
            // Get unenriched standard fields
            var standardFields = await dbContext.FieldDefinitions
                .Where(f => f.EnrichedAt == null)
                .ToListAsync(cancellationToken);

            // Get unenriched custom fields
            var customFields = await dbContext.CustomFieldDescriptors
                .Where(f => f.EnrichedAt == null)
                .ToListAsync(cancellationToken);

            int totalProcessed = 0;
            int successfullyEnriched = 0;
            int failed = 0;

            logger.LogInformation(
                "Starting enrichment: {StandardCount} standard fields, {CustomCount} custom fields",
                standardFields.Count, customFields.Count);

            // Enrich standard fields
            foreach (var field in standardFields)
            {
                totalProcessed++;
                var result = await EnrichStandardFieldAsync(field, cancellationToken);
                if (result.IsSuccess)
                    successfullyEnriched++;
                else
                    failed++;
            }

            // Enrich custom fields
            foreach (var field in customFields)
            {
                totalProcessed++;
                var result = await EnrichCustomFieldAsync(field, cancellationToken);
                if (result.IsSuccess)
                    successfullyEnriched++;
                else
                    failed++;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            var elapsedMs = (DateTime.UtcNow - startTime).TotalMilliseconds;
            metrics.RecordLatency("enrichment.enrich-all", elapsedMs);

            logger.LogInformation(
                "Enrichment complete: {Success} successful, {Failed} failed out of {Total}",
                successfullyEnriched, failed, totalProcessed);

            return Result<EnrichmentResult>.Success(new EnrichmentResult
            {
                TotalProcessed = totalProcessed,
                SuccessfullyEnriched = successfullyEnriched,
                Failed = failed,
                Skipped = 0
            });
        }
        catch (Exception ex)
        {
            metrics.IncrementCounter("enrichment.enrich-all.errors");
            logger.LogError(ex, "Error during field enrichment");
            return Result<EnrichmentResult>.Failure($"Enrichment failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Enriches a specific field definition.
    /// </summary>
    public async Task<Result<bool>> EnrichFieldAsync(
        string recordType,
        string fieldId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Try standard field first
            var standardField = await dbContext.FieldDefinitions
                .FirstOrDefaultAsync(f => f.RecordType == recordType && f.FieldId == fieldId,
                    cancellationToken);

            if (standardField != null)
            {
                var result = await EnrichStandardFieldAsync(standardField, cancellationToken);
                if (result.IsSuccess)
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                return result;
            }

            // Try custom field
            var customField = await dbContext.CustomFieldDescriptors
                .FirstOrDefaultAsync(f => f.RecordType == recordType && f.FieldId == fieldId,
                    cancellationToken);

            if (customField != null)
            {
                var result = await EnrichCustomFieldAsync(customField, cancellationToken);
                if (result.IsSuccess)
                {
                    await dbContext.SaveChangesAsync(cancellationToken);
                }
                return result;
            }

            return Result<bool>.Failure($"Field not found: {recordType}.{fieldId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enriching field {RecordType}.{FieldId}",
                recordType, fieldId);
            return Result<bool>.Failure($"Enrichment failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets enrichment status including count of enriched vs total fields.
    /// </summary>
    public async Task<Result<EnrichmentStatus>> GetEnrichmentStatusAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            var totalStandard = await dbContext.FieldDefinitions.CountAsync(cancellationToken);
            var enrichedStandard = await dbContext.FieldDefinitions
                .CountAsync(f => f.EnrichedAt != null, cancellationToken);

            var totalCustom = await dbContext.CustomFieldDescriptors.CountAsync(cancellationToken);
            var enrichedCustom = await dbContext.CustomFieldDescriptors
                .CountAsync(f => f.EnrichedAt != null, cancellationToken);

            var total = totalStandard + totalCustom;
            var enriched = enrichedStandard + enrichedCustom;

            var percentage = total > 0 ? (decimal)enriched / total * 100 : 0;

            var lastEnrichment = await dbContext.FieldDefinitions
                .Where(f => f.EnrichedAt != null)
                .OrderByDescending(f => f.EnrichedAt)
                .Select(f => f.EnrichedAt)
                .FirstOrDefaultAsync(cancellationToken);

            return Result<EnrichmentStatus>.Success(new EnrichmentStatus
            {
                TotalFields = total,
                EnrichedFields = enriched,
                EnrichmentPercentage = Math.Round(percentage, 2),
                LastEnrichmentRun = lastEnrichment
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting enrichment status");
            return Result<EnrichmentStatus>.Failure($"Status retrieval failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Manually updates enrichment data for a specific field.
    /// </summary>
    public async Task<Result<bool>> UpdateFieldEnrichmentAsync(
        string recordType,
        string fieldId,
        List<string>? aliases,
        string? businessContext,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Update standard field
            var standardField = await dbContext.FieldDefinitions
                .FirstOrDefaultAsync(f => f.RecordType == recordType && f.FieldId == fieldId,
                    cancellationToken);

            if (standardField != null)
            {
                // Create new instance with updated properties
                var updatedField = new FieldDefinition
                {
                    Id = standardField.Id,
                    RecordType = standardField.RecordType,
                    FieldId = standardField.FieldId,
                    Label = standardField.Label,
                    FieldType = standardField.FieldType,
                    Description = standardField.Description,
                    IsCustomField = standardField.IsCustomField,
                    ValidOperators = standardField.ValidOperators,
                    RequiredJoin = standardField.RequiredJoin,
                    Aliases = aliases ?? standardField.Aliases,
                    SupportsFilter = standardField.SupportsFilter,
                    SupportsColumn = standardField.SupportsColumn,
                    SupportsSummary = standardField.SupportsSummary,
                    CreatedAt = standardField.CreatedAt,
                    UpdatedAt = DateTime.UtcNow,
                    SchemaUrl = standardField.SchemaUrl,
                    Source = standardField.Source,
                    BusinessContext = businessContext ?? standardField.BusinessContext,
                    AmbiguityScore = standardField.AmbiguityScore,
                    Embedding = standardField.Embedding,
                    EnrichedAt = standardField.EnrichedAt
                };

                dbContext.Entry(standardField).CurrentValues.SetValues(updatedField);
                await dbContext.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }

            // Update custom field
            var customField = await dbContext.CustomFieldDescriptors
                .FirstOrDefaultAsync(f => f.RecordType == recordType && f.FieldId == fieldId,
                    cancellationToken);

            if (customField != null)
            {
                if (aliases != null) customField.Aliases = aliases;
                if (businessContext != null) customField.BusinessContext = businessContext;

                await dbContext.SaveChangesAsync(cancellationToken);
                return Result<bool>.Success(true);
            }

            return Result<bool>.Failure($"Field not found: {recordType}.{fieldId}");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating field enrichment");
            return Result<bool>.Failure($"Update failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Enriches a standard field with aliases, business context, and embedding.
    /// </summary>
    private async Task<Result<bool>> EnrichStandardFieldAsync(
        FieldDefinition field,
        CancellationToken cancellationToken)
    {
        try
        {
            // Generate aliases if not present
            var aliases = field.Aliases ?? GenerateAliases(field.FieldId, field.Label);

            // Generate business context
            var businessContext = GenerateBusinessContext(field);

            // Generate embedding text
            var embeddingText = $"{field.Label} {field.Description ?? ""} {string.Join(" ", aliases)}";

            // Generate embedding
            var embeddingResult = await embeddingService.GenerateEmbeddingAsync(
                embeddingText,
                cancellationToken);

            if (!embeddingResult.IsSuccess)
            {
                logger.LogWarning("Failed to generate embedding for {FieldId}: {Error}",
                    field.FieldId, embeddingResult.Error);
                return Result<bool>.Failure(embeddingResult.Error!);
            }

            // Calculate ambiguity score (placeholder - would compare with other fields)
            var ambiguityScore = 0.0m;

            // Update field with enrichment data
            var updatedField = new FieldDefinition
            {
                Id = field.Id,
                RecordType = field.RecordType,
                FieldId = field.FieldId,
                Label = field.Label,
                FieldType = field.FieldType,
                Description = field.Description,
                IsCustomField = field.IsCustomField,
                ValidOperators = field.ValidOperators,
                RequiredJoin = field.RequiredJoin,
                Aliases = aliases,
                SupportsFilter = field.SupportsFilter,
                SupportsColumn = field.SupportsColumn,
                SupportsSummary = field.SupportsSummary,
                CreatedAt = field.CreatedAt,
                UpdatedAt = DateTime.UtcNow,
                SchemaUrl = field.SchemaUrl,
                Source = field.Source,
                BusinessContext = JsonSerializer.Serialize(businessContext),
                AmbiguityScore = ambiguityScore,
                Embedding = embeddingResult.Value,
                EnrichedAt = DateTime.UtcNow
            };

            dbContext.Entry(field).CurrentValues.SetValues(updatedField);

            logger.LogDebug("Enriched field {FieldId} with {AliasCount} aliases",
                field.FieldId, aliases?.Count ?? 0);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enriching standard field {FieldId}", field.FieldId);
            return Result<bool>.Failure($"Enrichment failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Enriches a custom field with aliases, business context, and embedding.
    /// </summary>
    private async Task<Result<bool>> EnrichCustomFieldAsync(
        CustomFieldDescriptor field,
        CancellationToken cancellationToken)
    {
        try
        {
            // Generate aliases
            field.Aliases = GenerateAliases(field.FieldId, field.Label);

            // Generate business context
            var businessContext = new Dictionary<string, object>
            {
                ["label"] = field.Label,
                ["type"] = field.Type,
                ["recordType"] = field.RecordType,
                ["isMandatory"] = field.IsMandatory
            };

            if (!string.IsNullOrEmpty(field.Description))
            {
                businessContext["description"] = field.Description;
            }

            field.BusinessContext = JsonSerializer.Serialize(businessContext);

            // Generate embedding text
            var embeddingText = $"{field.Label} {field.Description ?? ""} {string.Join(" ", field.Aliases)}";

            // Generate embedding
            var embeddingResult = await embeddingService.GenerateEmbeddingAsync(
                embeddingText,
                cancellationToken);

            if (!embeddingResult.IsSuccess)
            {
                logger.LogWarning("Failed to generate embedding for {FieldId}: {Error}",
                    field.FieldId, embeddingResult.Error);
                return Result<bool>.Failure(embeddingResult.Error!);
            }

            field.Embedding = embeddingResult.Value;
            field.AmbiguityScore = 0.0m;
            field.EnrichedAt = DateTime.UtcNow;

            logger.LogDebug("Enriched custom field {FieldId} with {AliasCount} aliases",
                field.FieldId, field.Aliases?.Count ?? 0);

            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error enriching custom field {FieldId}", field.FieldId);
            return Result<bool>.Failure($"Enrichment failed: {ex.Message}");
        }
    }

    /// <summary>
    /// Generates aliases for a field based on its ID and label.
    /// </summary>
    private static List<string> GenerateAliases(string fieldId, string label)
    {
        var aliases = new List<string>();

        // Add the label itself
        if (!string.IsNullOrWhiteSpace(label))
        {
            aliases.Add(label.ToLower());
        }

        // Common NetSuite field mappings
        var commonMappings = new Dictionary<string, List<string>>
        {
            ["trandate"] = ["transaction date", "date", "trans date", "posting date"],
            ["entity"] = ["customer", "vendor", "contact", "company", "name"],
            ["amount"] = ["total", "value", "sum"],
            ["subsidiary"] = ["sub", "company", "entity"],
            ["department"] = ["dept", "division"],
            ["class"] = ["classification", "category"],
            ["location"] = ["loc", "site", "branch"],
            ["memo"] = ["notes", "description", "comments"],
            ["tranid"] = ["transaction number", "trans number", "document number", "trans id"],
            ["status"] = ["state", "condition"],
            ["type"] = ["transaction type", "record type"],
            ["currency"] = ["curr", "currency code"]
        };

        if (commonMappings.TryGetValue(fieldId.ToLower(), out var mappings))
        {
            aliases.AddRange(mappings);
        }

        // Extract words from camelCase or underscore-separated field IDs
        var words = System.Text.RegularExpressions.Regex
            .Replace(fieldId, "([a-z])([A-Z])", "$1 $2")
            .Replace("_", " ")
            .Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var word in words)
        {
            if (word.Length > 2 && !aliases.Contains(word.ToLower()))
            {
                aliases.Add(word.ToLower());
            }
        }

        return aliases.Distinct().ToList();
    }

    /// <summary>
    /// Generates business context for a field.
    /// </summary>
    private static Dictionary<string, object> GenerateBusinessContext(FieldDefinition field)
    {
        var context = new Dictionary<string, object>
        {
            ["label"] = field.Label,
            ["fieldId"] = field.FieldId,
            ["recordType"] = field.RecordType,
            ["fieldType"] = field.FieldType.ToString(),
            ["supportsFilter"] = field.SupportsFilter,
            ["supportsColumn"] = field.SupportsColumn,
            ["supportsSummary"] = field.SupportsSummary
        };

        if (!string.IsNullOrEmpty(field.Description))
        {
            context["description"] = field.Description;
        }

        if (!string.IsNullOrEmpty(field.RequiredJoin))
        {
            context["requiredJoin"] = field.RequiredJoin;
        }

        if (field.ValidOperators.Any())
        {
            context["validOperators"] = field.ValidOperators.Select(o => o.ToString()).ToList();
        }

        // Add usage examples based on field type
        context["usageExamples"] = field.FieldType switch
        {
            FieldType.Date => new[] { "last month", "this year", "after 2024-01-01" },
            FieldType.Currency or FieldType.Number or FieldType.Percent =>
                new[] { "greater than 1000", "between 100 and 500" },
            FieldType.Text => new[] { "contains 'invoice'", "starts with 'A'" },
            _ => new[] { "equals value", "is not empty" }
        };

        return context;
    }
}
