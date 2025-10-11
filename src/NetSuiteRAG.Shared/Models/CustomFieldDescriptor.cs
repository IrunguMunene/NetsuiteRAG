namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents a NetSuite custom field descriptor with metadata.
/// </summary>
public class CustomFieldDescriptor
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// NetSuite field ID (e.g., "custbody_payment_terms").
    /// </summary>
    public required string FieldId { get; set; }

    /// <summary>
    /// Record type this field belongs to (e.g., "transaction", "customer").
    /// </summary>
    public required string RecordType { get; set; }

    /// <summary>
    /// Display label for the field.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Field data type (uses FieldType enum values as string).
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Help text or description.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Whether this field is mandatory.
    /// </summary>
    public bool IsMandatory { get; set; }

    /// <summary>
    /// Default value if any.
    /// </summary>
    public string? DefaultValue { get; set; }

    /// <summary>
    /// Whether this field is stale (not seen in 90+ days).
    /// </summary>
    public bool IsStale { get; set; }

    /// <summary>
    /// When this field was first discovered.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When this field was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// When this field was last seen during a crawl.
    /// </summary>
    public DateTime LastSeenAt { get; set; }

    /// <summary>
    /// Additional metadata as JSON.
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// Alternative field names or aliases for natural language matching.
    /// </summary>
    public List<string>? Aliases { get; set; }

    /// <summary>
    /// Business context including usage scenarios and examples (stored as JSONB).
    /// </summary>
    public string? BusinessContext { get; set; }

    /// <summary>
    /// Ambiguity score (0.00 to 1.00) indicating similarity to other fields.
    /// </summary>
    public decimal? AmbiguityScore { get; set; }

    /// <summary>
    /// Embedding vector for semantic search (768 dimensions for nomic-embed-text).
    /// Not stored in PostgreSQL - indexed in Qdrant vector database (T1.05).
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public float[]? Embedding { get; set; }

    /// <summary>
    /// Timestamp when this field was enriched with aliases, context, and embeddings.
    /// </summary>
    public DateTime? EnrichedAt { get; set; }
}
