namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents a business glossary term with synonyms and domain knowledge.
/// Used for RAG retrieval to help LLM understand NetSuite terminology.
/// </summary>
public class GlossaryTerm
{
    /// <summary>
    /// Unique identifier for the glossary term.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// The primary term or concept.
    /// </summary>
    public required string Term { get; set; }

    /// <summary>
    /// Definition or explanation of the term.
    /// </summary>
    public required string Definition { get; set; }

    /// <summary>
    /// List of synonyms or alternative names for this term.
    /// </summary>
    public List<string> Synonyms { get; set; } = [];

    /// <summary>
    /// Category of the term (e.g., "accounting", "fields", "operations").
    /// </summary>
    public required string Category { get; set; }

    /// <summary>
    /// Example usage of the term in context.
    /// </summary>
    public List<string> Examples { get; set; } = [];

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
    /// When the term was created.
    /// </summary>
    public required DateTime CreatedAt { get; init; }

    /// <summary>
    /// When the term was last updated.
    /// </summary>
    public required DateTime UpdatedAt { get; set; }

    /// <summary>
    /// When the embedding was last generated.
    /// </summary>
    public DateTime? EmbeddedAt { get; set; }

    /// <summary>
    /// Whether this term is active and should be included in RAG retrieval.
    /// </summary>
    public bool IsActive { get; set; } = true;
}
