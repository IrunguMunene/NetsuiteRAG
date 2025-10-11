using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Interfaces;

/// <summary>
/// Service interface for managing business glossary terms and query exemplars.
/// Provides CRUD operations and embedding generation for RAG retrieval.
/// </summary>
public interface IGlossaryService
{
    // ========== Glossary Terms ==========

    /// <summary>
    /// Gets all glossary terms, optionally filtered by category and active status.
    /// </summary>
    /// <param name="category">Optional category filter.</param>
    /// <param name="activeOnly">Whether to return only active terms.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of glossary terms.</returns>
    Task<Result<List<GlossaryTerm>>> GetAllTermsAsync(
        string? category = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a glossary term by ID.
    /// </summary>
    /// <param name="id">Term ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Glossary term if found; otherwise, failure.</returns>
    Task<Result<GlossaryTerm>> GetTermByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches glossary terms by keyword (searches term, definition, synonyms).
    /// </summary>
    /// <param name="keyword">Search keyword.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching glossary terms.</returns>
    Task<Result<List<GlossaryTerm>>> SearchTermsAsync(
        string keyword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new glossary term.
    /// </summary>
    /// <param name="term">Term text.</param>
    /// <param name="definition">Definition.</param>
    /// <param name="synonyms">List of synonyms.</param>
    /// <param name="category">Category.</param>
    /// <param name="examples">List of examples.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created glossary term.</returns>
    Task<Result<GlossaryTerm>> CreateTermAsync(
        string term,
        string definition,
        List<string> synonyms,
        string category,
        List<string> examples,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing glossary term.
    /// </summary>
    /// <param name="id">Term ID.</param>
    /// <param name="term">Updated term text.</param>
    /// <param name="definition">Updated definition.</param>
    /// <param name="synonyms">Updated list of synonyms.</param>
    /// <param name="category">Updated category.</param>
    /// <param name="examples">Updated list of examples.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated glossary term.</returns>
    Task<Result<GlossaryTerm>> UpdateTermAsync(
        Guid id,
        string term,
        string definition,
        List<string> synonyms,
        string category,
        List<string> examples,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a glossary term (soft delete by setting IsActive = false).
    /// </summary>
    /// <param name="id">Term ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result<bool>> DeleteTermAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    // ========== Query Exemplars ==========

    /// <summary>
    /// Gets all query exemplars, optionally filtered by record type, difficulty, and active status.
    /// </summary>
    /// <param name="recordType">Optional record type filter.</param>
    /// <param name="difficulty">Optional difficulty filter.</param>
    /// <param name="activeOnly">Whether to return only active exemplars.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of query exemplars.</returns>
    Task<Result<List<QueryExemplar>>> GetAllExemplarsAsync(
        string? recordType = null,
        DifficultyLevel? difficulty = null,
        bool activeOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a query exemplar by ID.
    /// </summary>
    /// <param name="id">Exemplar ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Query exemplar if found; otherwise, failure.</returns>
    Task<Result<QueryExemplar>> GetExemplarByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches query exemplars by keyword (searches natural query, explanation, tags).
    /// </summary>
    /// <param name="keyword">Search keyword.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of matching query exemplars.</returns>
    Task<Result<List<QueryExemplar>>> SearchExemplarsAsync(
        string keyword,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new query exemplar.
    /// </summary>
    /// <param name="naturalQuery">Natural language query.</param>
    /// <param name="savedSearchPlanJson">SavedSearchPlan JSON.</param>
    /// <param name="explanation">Explanation.</param>
    /// <param name="tags">List of tags.</param>
    /// <param name="difficulty">Difficulty level.</param>
    /// <param name="recordType">Record type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Created query exemplar.</returns>
    Task<Result<QueryExemplar>> CreateExemplarAsync(
        string naturalQuery,
        string savedSearchPlanJson,
        string explanation,
        List<string> tags,
        DifficultyLevel difficulty,
        string recordType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing query exemplar.
    /// </summary>
    /// <param name="id">Exemplar ID.</param>
    /// <param name="naturalQuery">Updated natural language query.</param>
    /// <param name="savedSearchPlanJson">Updated SavedSearchPlan JSON.</param>
    /// <param name="explanation">Updated explanation.</param>
    /// <param name="tags">Updated list of tags.</param>
    /// <param name="difficulty">Updated difficulty level.</param>
    /// <param name="recordType">Updated record type.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Updated query exemplar.</returns>
    Task<Result<QueryExemplar>> UpdateExemplarAsync(
        Guid id,
        string naturalQuery,
        string savedSearchPlanJson,
        string explanation,
        List<string> tags,
        DifficultyLevel difficulty,
        string recordType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a query exemplar (soft delete by setting IsActive = false).
    /// </summary>
    /// <param name="id">Exemplar ID.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Success or failure.</returns>
    Task<Result<bool>> DeleteExemplarAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    // ========== Embedding Operations ==========

    /// <summary>
    /// Generates embeddings for all glossary terms that don't have embeddings yet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with count of terms processed.</returns>
    Task<Result<int>> GenerateTermEmbeddingsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates embeddings for all query exemplars that don't have embeddings yet.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Result with count of exemplars processed.</returns>
    Task<Result<int>> GenerateExemplarEmbeddingsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets statistics about the glossary (term count, exemplar count, embedding coverage).
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Glossary statistics.</returns>
    Task<Result<GlossaryStatistics>> GetStatisticsAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Statistics about the business glossary.
/// </summary>
public record GlossaryStatistics
{
    public required int TotalTerms { get; init; }
    public required int ActiveTerms { get; init; }
    public required int TermsWithEmbeddings { get; init; }
    public required int TotalExemplars { get; init; }
    public required int ActiveExemplars { get; init; }
    public required int ExemplarsWithEmbeddings { get; init; }
    public required Dictionary<string, int> TermsByCategory { get; init; }
    public required Dictionary<string, int> ExemplarsByRecordType { get; init; }
    public required Dictionary<string, int> ExemplarsByDifficulty { get; init; }
}
