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
}
