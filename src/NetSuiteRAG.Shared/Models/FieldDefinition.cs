namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents a NetSuite field definition with metadata for query planning and validation.
/// Fields can be standard (built-in) or custom (tenant-specific).
/// </summary>
public class FieldDefinition
{
    /// <summary>
    /// Gets or initializes the unique identifier for the field definition.
    /// </summary>
    public Guid Id { get; init; }

    /// <summary>
    /// Gets or initializes the NetSuite record type (e.g., "transaction", "customer", "vendor").
    /// Maps to the base search object (e.g., TransactionSearch, CustomerSearch).
    /// </summary>
    public required string RecordType { get; init; }

    /// <summary>
    /// Gets or initializes the field identifier used in NetSuite searches (e.g., "trandate", "entity", "amount").
    /// This is the internal ID used in filter and column definitions.
    /// </summary>
    public required string FieldId { get; init; }

    /// <summary>
    /// Gets or initializes the human-readable field label (e.g., "Transaction Date", "Entity", "Amount").
    /// Used for natural language matching and UI display.
    /// </summary>
    public required string Label { get; init; }

    /// <summary>
    /// Gets or initializes the field data type.
    /// Determines which operators are valid and how values should be formatted.
    /// </summary>
    public required FieldType FieldType { get; init; }

    /// <summary>
    /// Gets or initializes an optional description of the field's purpose and usage.
    /// Enhances RAG context for better field matching.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or initializes a value indicating whether this is a custom field (true) or standard field (false).
    /// Custom fields typically have prefixes like "custbody", "custcol", "custentity", "custitem".
    /// </summary>
    public bool IsCustomField { get; init; }

    /// <summary>
    /// Gets or initializes the list of valid operators for this field.
    /// Derived from the operator compatibility matrix based on FieldType.
    /// </summary>
    public required List<OperatorType> ValidOperators { get; init; }

    /// <summary>
    /// Gets or initializes the join name required to access this field, if not on the base record.
    /// Null if the field is available on the base SearchBasic/SearchRowBasic object.
    /// Examples: "vendorJoin", "customerJoin", "subsidiaryJoin"
    /// </summary>
    public string? RequiredJoin { get; init; }

    /// <summary>
    /// Gets or initializes alternative field names or aliases for natural language matching.
    /// Examples: ["vendor", "supplier"] for entity field, ["class", "classification"] for classification.
    /// </summary>
    public List<string>? Aliases { get; init; }

    /// <summary>
    /// Gets or initializes a value indicating whether this field can be used as a filter.
    /// Most fields support filtering, but some display-only fields may not.
    /// </summary>
    public bool SupportsFilter { get; init; } = true;

    /// <summary>
    /// Gets or initializes a value indicating whether this field can be used as a column (result field).
    /// All fields should support being displayed as columns.
    /// </summary>
    public bool SupportsColumn { get; init; } = true;

    /// <summary>
    /// Gets or initializes a value indicating whether this field can be used in summaries (grouping/aggregation).
    /// Typically true for dimension fields (text, list) and measure fields (number, currency).
    /// </summary>
    public bool SupportsSummary { get; init; }

    /// <summary>
    /// Gets or initializes the timestamp when this field definition was created.
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or initializes the timestamp when this field definition was last updated.
    /// Used for custom field refresh tracking.
    /// </summary>
    public DateTime? UpdatedAt { get; init; }

    /// <summary>
    /// Gets or initializes reference URL to NetSuite schema browser documentation.
    /// Helps developers verify field behavior and constraints.
    /// </summary>
    public string? SchemaUrl { get; init; }

    /// <summary>
    /// Gets or initializes the source of this field definition (e.g., "catalog-2025.1", "custom-crawler").
    /// Used for audit and troubleshooting.
    /// </summary>
    public required string Source { get; init; }

    /// <summary>
    /// Gets or initializes the business context for this field including usage scenarios and examples.
    /// Stored as JSONB in the database for flexible metadata.
    /// </summary>
    public string? BusinessContext { get; init; }

    /// <summary>
    /// Gets or initializes the ambiguity score (0.00 to 1.00) indicating similarity to other fields.
    /// Higher scores indicate potential confusion with similarly named fields.
    /// </summary>
    public decimal? AmbiguityScore { get; init; }

    /// <summary>
    /// Gets or initializes the embedding vector for semantic search (768 dimensions for nomic-embed-text).
    /// Generated from field label, description, aliases, and business context.
    /// Not stored in PostgreSQL - indexed in Qdrant vector database (T1.05).
    /// </summary>
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public float[]? Embedding { get; init; }

    /// <summary>
    /// Gets or initializes the timestamp when this field was enriched with aliases, context, and embeddings.
    /// </summary>
    public DateTime? EnrichedAt { get; init; }
}
