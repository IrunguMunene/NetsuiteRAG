using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetSuiteRAG.Api.Data;
using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Implementations;

/// <summary>
/// Service for managing business glossary terms and query exemplars.
/// Provides CRUD operations for RAG retrieval.
/// </summary>
public class GlossaryService(
    AppDbContext context,
    ILogger<GlossaryService> logger) : IGlossaryService
{
    // ========== Glossary Terms ==========

    /// <summary>
    /// Gets all glossary terms, optionally filtered by category and active status.
    /// </summary>
    public async Task<Result<List<GlossaryTerm>>> GetAllTermsAsync(
        string? category = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving glossary terms. Category: {Category}, ActiveOnly: {ActiveOnly}",
                category ?? "All", activeOnly);

            var query = context.GlossaryTerms.AsQueryable();

            if (activeOnly)
                query = query.Where(t => t.IsActive);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(t => t.Category == category);

            var terms = await query
                .OrderBy(t => t.Category)
                .ThenBy(t => t.Term)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} glossary terms", terms.Count);
            return Result<List<GlossaryTerm>>.Success(terms);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving glossary terms");
            return Result<List<GlossaryTerm>>.Failure($"Failed to retrieve glossary terms: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a glossary term by ID.
    /// </summary>
    public async Task<Result<GlossaryTerm>> GetTermByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var term = await context.GlossaryTerms
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (term == null)
            {
                logger.LogWarning("Glossary term not found: {TermId}", id);
                return Result<GlossaryTerm>.Failure($"Glossary term not found: {id}");
            }

            return Result<GlossaryTerm>.Success(term);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving glossary term: {TermId}", id);
            return Result<GlossaryTerm>.Failure($"Failed to retrieve glossary term: {ex.Message}");
        }
    }

    /// <summary>
    /// Searches glossary terms by keyword (searches term, definition, synonyms).
    /// </summary>
    public async Task<Result<List<GlossaryTerm>>> SearchTermsAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Result<List<GlossaryTerm>>.Failure("Keyword cannot be empty");

            logger.LogInformation("Searching glossary terms with keyword: {Keyword}", keyword);

            var lowerKeyword = keyword.ToLower();

            var terms = await context.GlossaryTerms
                .Where(t => t.IsActive &&
                    (t.Term.ToLower().Contains(lowerKeyword) ||
                     t.Definition.ToLower().Contains(lowerKeyword) ||
                     t.Synonyms.Any(s => s.ToLower().Contains(lowerKeyword))))
                .OrderBy(t => t.Term)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Found {Count} matching glossary terms", terms.Count);
            return Result<List<GlossaryTerm>>.Success(terms);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching glossary terms");
            return Result<List<GlossaryTerm>>.Failure($"Failed to search glossary terms: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new glossary term.
    /// </summary>
    public async Task<Result<GlossaryTerm>> CreateTermAsync(
        string term,
        string definition,
        List<string> synonyms,
        string category,
        List<string> examples,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(term))
                return Result<GlossaryTerm>.Failure("Term cannot be empty");

            if (string.IsNullOrWhiteSpace(definition))
                return Result<GlossaryTerm>.Failure("Definition cannot be empty");

            if (string.IsNullOrWhiteSpace(category))
                return Result<GlossaryTerm>.Failure("Category cannot be empty");

            // Check for duplicate term
            var exists = await context.GlossaryTerms
                .AnyAsync(t => t.Term.ToLower() == term.ToLower() && t.IsActive, cancellationToken);

            if (exists)
                return Result<GlossaryTerm>.Failure($"Glossary term already exists: {term}");

            logger.LogInformation("Creating glossary term: {Term}", term);

            var now = DateTime.UtcNow;
            var glossaryTerm = new GlossaryTerm
            {
                Id = Guid.NewGuid(),
                Term = term,
                Definition = definition,
                Synonyms = synonyms ?? [],
                Category = category,
                Examples = examples ?? [],
                CreatedAt = now,
                UpdatedAt = now,
                IsActive = true
            };

            context.GlossaryTerms.Add(glossaryTerm);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Created glossary term: {TermId} - {Term}", glossaryTerm.Id, term);
            return Result<GlossaryTerm>.Success(glossaryTerm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating glossary term: {Term}", term);
            return Result<GlossaryTerm>.Failure($"Failed to create glossary term: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing glossary term.
    /// </summary>
    public async Task<Result<GlossaryTerm>> UpdateTermAsync(
        Guid id,
        string term,
        string definition,
        List<string> synonyms,
        string category,
        List<string> examples,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(term))
                return Result<GlossaryTerm>.Failure("Term cannot be empty");

            if (string.IsNullOrWhiteSpace(definition))
                return Result<GlossaryTerm>.Failure("Definition cannot be empty");

            if (string.IsNullOrWhiteSpace(category))
                return Result<GlossaryTerm>.Failure("Category cannot be empty");

            var glossaryTerm = await context.GlossaryTerms
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (glossaryTerm == null)
            {
                logger.LogWarning("Glossary term not found for update: {TermId}", id);
                return Result<GlossaryTerm>.Failure($"Glossary term not found: {id}");
            }

            // Check for duplicate term (excluding current term)
            var exists = await context.GlossaryTerms
                .AnyAsync(t => t.Id != id && t.Term.ToLower() == term.ToLower() && t.IsActive, cancellationToken);

            if (exists)
                return Result<GlossaryTerm>.Failure($"Glossary term already exists: {term}");

            logger.LogInformation("Updating glossary term: {TermId} - {Term}", id, term);

            glossaryTerm.Term = term;
            glossaryTerm.Definition = definition;
            glossaryTerm.Synonyms = synonyms ?? [];
            glossaryTerm.Category = category;
            glossaryTerm.Examples = examples ?? [];
            glossaryTerm.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Updated glossary term: {TermId} - {Term}", id, term);
            return Result<GlossaryTerm>.Success(glossaryTerm);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating glossary term: {TermId}", id);
            return Result<GlossaryTerm>.Failure($"Failed to update glossary term: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a glossary term (soft delete by setting IsActive = false).
    /// </summary>
    public async Task<Result<bool>> DeleteTermAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var term = await context.GlossaryTerms
                .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

            if (term == null)
            {
                logger.LogWarning("Glossary term not found for deletion: {TermId}", id);
                return Result<bool>.Failure($"Glossary term not found: {id}");
            }

            logger.LogInformation("Soft deleting glossary term: {TermId} - {Term}", id, term.Term);

            term.IsActive = false;
            term.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Deleted glossary term: {TermId} - {Term}", id, term.Term);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting glossary term: {TermId}", id);
            return Result<bool>.Failure($"Failed to delete glossary term: {ex.Message}");
        }
    }

    // ========== Query Exemplars ==========

    /// <summary>
    /// Gets all query exemplars, optionally filtered by record type, difficulty, and active status.
    /// </summary>
    public async Task<Result<List<QueryExemplar>>> GetAllExemplarsAsync(
        string? recordType = null,
        DifficultyLevel? difficulty = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving query exemplars. RecordType: {RecordType}, Difficulty: {Difficulty}, ActiveOnly: {ActiveOnly}",
                recordType ?? "All", difficulty?.ToString() ?? "All", activeOnly);

            var query = context.QueryExemplars.AsQueryable();

            if (activeOnly)
                query = query.Where(e => e.IsActive);

            if (!string.IsNullOrWhiteSpace(recordType))
                query = query.Where(e => e.RecordType == recordType);

            if (difficulty.HasValue)
                query = query.Where(e => e.Difficulty == difficulty.Value);

            var exemplars = await query
                .OrderBy(e => e.Difficulty)
                .ThenBy(e => e.RecordType)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Retrieved {Count} query exemplars", exemplars.Count);
            return Result<List<QueryExemplar>>.Success(exemplars);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving query exemplars");
            return Result<List<QueryExemplar>>.Failure($"Failed to retrieve query exemplars: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets a query exemplar by ID.
    /// </summary>
    public async Task<Result<QueryExemplar>> GetExemplarByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var exemplar = await context.QueryExemplars
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (exemplar == null)
            {
                logger.LogWarning("Query exemplar not found: {ExemplarId}", id);
                return Result<QueryExemplar>.Failure($"Query exemplar not found: {id}");
            }

            return Result<QueryExemplar>.Success(exemplar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving query exemplar: {ExemplarId}", id);
            return Result<QueryExemplar>.Failure($"Failed to retrieve query exemplar: {ex.Message}");
        }
    }

    /// <summary>
    /// Searches query exemplars by keyword (searches natural query, explanation, tags).
    /// </summary>
    public async Task<Result<List<QueryExemplar>>> SearchExemplarsAsync(
        string keyword,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(keyword))
                return Result<List<QueryExemplar>>.Failure("Keyword cannot be empty");

            logger.LogInformation("Searching query exemplars with keyword: {Keyword}", keyword);

            var lowerKeyword = keyword.ToLower();

            var exemplars = await context.QueryExemplars
                .Where(e => e.IsActive &&
                    (e.NaturalQuery.ToLower().Contains(lowerKeyword) ||
                     e.Explanation.ToLower().Contains(lowerKeyword) ||
                     e.Tags.Any(t => t.ToLower().Contains(lowerKeyword))))
                .OrderBy(e => e.Difficulty)
                .ThenBy(e => e.RecordType)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Found {Count} matching query exemplars", exemplars.Count);
            return Result<List<QueryExemplar>>.Success(exemplars);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching query exemplars");
            return Result<List<QueryExemplar>>.Failure($"Failed to search query exemplars: {ex.Message}");
        }
    }

    /// <summary>
    /// Creates a new query exemplar.
    /// </summary>
    public async Task<Result<QueryExemplar>> CreateExemplarAsync(
        string naturalQuery,
        string savedSearchPlanJson,
        string explanation,
        List<string> tags,
        DifficultyLevel difficulty,
        string recordType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(naturalQuery))
                return Result<QueryExemplar>.Failure("Natural query cannot be empty");

            if (string.IsNullOrWhiteSpace(savedSearchPlanJson))
                return Result<QueryExemplar>.Failure("SavedSearchPlan JSON cannot be empty");

            if (string.IsNullOrWhiteSpace(explanation))
                return Result<QueryExemplar>.Failure("Explanation cannot be empty");

            if (string.IsNullOrWhiteSpace(recordType))
                return Result<QueryExemplar>.Failure("Record type cannot be empty");

            // Validate JSON format
            try
            {
                JsonDocument.Parse(savedSearchPlanJson);
            }
            catch (JsonException)
            {
                return Result<QueryExemplar>.Failure("SavedSearchPlan JSON is not valid JSON");
            }

            logger.LogInformation("Creating query exemplar: {Query}", naturalQuery);

            var now = DateTime.UtcNow;
            var exemplar = new QueryExemplar
            {
                Id = Guid.NewGuid(),
                NaturalQuery = naturalQuery,
                SavedSearchPlanJson = savedSearchPlanJson,
                Explanation = explanation,
                Tags = tags ?? [],
                Difficulty = difficulty,
                RecordType = recordType,
                CreatedAt = now,
                UpdatedAt = now,
                IsActive = true
            };

            context.QueryExemplars.Add(exemplar);
            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Created query exemplar: {ExemplarId} - {Query}", exemplar.Id, naturalQuery);
            return Result<QueryExemplar>.Success(exemplar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating query exemplar");
            return Result<QueryExemplar>.Failure($"Failed to create query exemplar: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates an existing query exemplar.
    /// </summary>
    public async Task<Result<QueryExemplar>> UpdateExemplarAsync(
        Guid id,
        string naturalQuery,
        string savedSearchPlanJson,
        string explanation,
        List<string> tags,
        DifficultyLevel difficulty,
        string recordType,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validation
            if (string.IsNullOrWhiteSpace(naturalQuery))
                return Result<QueryExemplar>.Failure("Natural query cannot be empty");

            if (string.IsNullOrWhiteSpace(savedSearchPlanJson))
                return Result<QueryExemplar>.Failure("SavedSearchPlan JSON cannot be empty");

            if (string.IsNullOrWhiteSpace(explanation))
                return Result<QueryExemplar>.Failure("Explanation cannot be empty");

            if (string.IsNullOrWhiteSpace(recordType))
                return Result<QueryExemplar>.Failure("Record type cannot be empty");

            // Validate JSON format
            try
            {
                JsonDocument.Parse(savedSearchPlanJson);
            }
            catch (JsonException)
            {
                return Result<QueryExemplar>.Failure("SavedSearchPlan JSON is not valid JSON");
            }

            var exemplar = await context.QueryExemplars
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (exemplar == null)
            {
                logger.LogWarning("Query exemplar not found for update: {ExemplarId}", id);
                return Result<QueryExemplar>.Failure($"Query exemplar not found: {id}");
            }

            logger.LogInformation("Updating query exemplar: {ExemplarId} - {Query}", id, naturalQuery);

            exemplar.NaturalQuery = naturalQuery;
            exemplar.SavedSearchPlanJson = savedSearchPlanJson;
            exemplar.Explanation = explanation;
            exemplar.Tags = tags ?? [];
            exemplar.Difficulty = difficulty;
            exemplar.RecordType = recordType;
            exemplar.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Updated query exemplar: {ExemplarId} - {Query}", id, naturalQuery);
            return Result<QueryExemplar>.Success(exemplar);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating query exemplar: {ExemplarId}", id);
            return Result<QueryExemplar>.Failure($"Failed to update query exemplar: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes a query exemplar (soft delete by setting IsActive = false).
    /// </summary>
    public async Task<Result<bool>> DeleteExemplarAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var exemplar = await context.QueryExemplars
                .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

            if (exemplar == null)
            {
                logger.LogWarning("Query exemplar not found for deletion: {ExemplarId}", id);
                return Result<bool>.Failure($"Query exemplar not found: {id}");
            }

            logger.LogInformation("Soft deleting query exemplar: {ExemplarId} - {Query}", id, exemplar.NaturalQuery);

            exemplar.IsActive = false;
            exemplar.UpdatedAt = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Deleted query exemplar: {ExemplarId} - {Query}", id, exemplar.NaturalQuery);
            return Result<bool>.Success(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting query exemplar: {ExemplarId}", id);
            return Result<bool>.Failure($"Failed to delete query exemplar: {ex.Message}");
        }
    }

    // ========== Statistics ==========

    /// <summary>
    /// Gets statistics about the glossary (term count, exemplar count by category/type).
    /// </summary>
    public async Task<Result<GlossaryStatistics>> GetStatisticsAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Retrieving glossary statistics");

            var totalTerms = await context.GlossaryTerms.CountAsync(cancellationToken);
            var activeTerms = await context.GlossaryTerms.CountAsync(t => t.IsActive, cancellationToken);

            var totalExemplars = await context.QueryExemplars.CountAsync(cancellationToken);
            var activeExemplars = await context.QueryExemplars.CountAsync(e => e.IsActive, cancellationToken);

            var termsByCategory = await context.GlossaryTerms
                .Where(t => t.IsActive)
                .GroupBy(t => t.Category)
                .Select(g => new { Category = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Category, x => x.Count, cancellationToken);

            var exemplarsByRecordType = await context.QueryExemplars
                .Where(e => e.IsActive)
                .GroupBy(e => e.RecordType)
                .Select(g => new { RecordType = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.RecordType, x => x.Count, cancellationToken);

            var exemplarsByDifficulty = await context.QueryExemplars
                .Where(e => e.IsActive)
                .GroupBy(e => e.Difficulty)
                .Select(g => new { Difficulty = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Difficulty.ToString(), x => x.Count, cancellationToken);

            var statistics = new GlossaryStatistics
            {
                TotalTerms = totalTerms,
                ActiveTerms = activeTerms,
                TotalExemplars = totalExemplars,
                ActiveExemplars = activeExemplars,
                TermsByCategory = termsByCategory,
                ExemplarsByRecordType = exemplarsByRecordType,
                ExemplarsByDifficulty = exemplarsByDifficulty
            };

            logger.LogInformation("Retrieved glossary statistics: {ActiveTerms} terms, {ActiveExemplars} exemplars",
                activeTerms, activeExemplars);

            return Result<GlossaryStatistics>.Success(statistics);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving glossary statistics");
            return Result<GlossaryStatistics>.Failure($"Failed to retrieve statistics: {ex.Message}");
        }
    }
}
