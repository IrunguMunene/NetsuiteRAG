namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents an exemplar query with its SavedSearchPlan solution.
/// Used for RAG retrieval to provide template solutions for common query patterns.
/// </summary>
public class QueryExemplar
{
    /// <summary>
    /// Unique identifier for the query exemplar.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Natural language query from the user.
    /// </summary>
    public required string NaturalQuery { get; set; }

    /// <summary>
    /// The SavedSearchPlan JSON that solves this query.
    /// </summary>
    public required string SavedSearchPlanJson { get; set; }

    /// <summary>
    /// Explanation of how the plan solves the query.
    /// Describes the approach, filters, joins, and any special considerations.
    /// </summary>
    public required string Explanation { get; set; }

    /// <summary>
    /// Tags for categorization (e.g., "transaction", "invoice", "date-range").
    /// </summary>
    public List<string> Tags { get; set; } = [];

    /// <summary>
    /// Difficulty level of the query.
    /// </summary>
    public required DifficultyLevel Difficulty { get; set; }

    /// <summary>
    /// Primary record type used in this exemplar (e.g., "transaction", "customer").
    /// </summary>
    public required string RecordType { get; set; }

    /// <summary>
    /// Additional metadata as JSON (flexible for future additions).
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Vector embedding for semantic search (768 dimensions for nomic-embed-text).
    /// Stored as JSON array, will be indexed in Qdrant.
    /// </summary>
    public string? EmbeddingJson { get; set; }

    /// <summary>
    /// When the exemplar was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the exemplar was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; set; }

    /// <summary>
    /// When the embedding was last generated.
    /// </summary>
    public DateTime? EmbeddedAt { get; set; }

    /// <summary>
    /// Whether this exemplar is active and should be included in RAG retrieval.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Number of times this exemplar has been retrieved in RAG context.
    /// Used for tracking effectiveness.
    /// </summary>
    public int RetrievalCount { get; set; } = 0;

    /// <summary>
    /// Last time this exemplar was retrieved.
    /// </summary>
    public DateTime? LastRetrievedAt { get; set; }
}

/// <summary>
/// Difficulty level for query exemplars.
/// </summary>
public enum DifficultyLevel
{
    /// <summary>
    /// Simple queries: Single record type, basic filters, no joins.
    /// </summary>
    Simple = 1,

    /// <summary>
    /// Moderate queries: With joins, date ranges, summaries.
    /// </summary>
    Moderate = 2,

    /// <summary>
    /// Complex queries: Multi-join, composition, advanced filters.
    /// </summary>
    Complex = 3
}
