namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Metadata for a NetSuite custom field returned from the API.
/// </summary>
public class NetSuiteCustomFieldMetadata
{
    /// <summary>
    /// Custom field ID (e.g., "custbody_payment_terms").
    /// </summary>
    public required string Id { get; set; }

    /// <summary>
    /// Display label.
    /// </summary>
    public required string Label { get; set; }

    /// <summary>
    /// Field type (text, select, date, etc.).
    /// </summary>
    public required string Type { get; set; }

    /// <summary>
    /// Record types this field applies to.
    /// </summary>
    public List<string>? RecordTypes { get; set; }

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
    /// Additional metadata.
    /// </summary>
    public Dictionary<string, object>? AdditionalProperties { get; set; }
}
