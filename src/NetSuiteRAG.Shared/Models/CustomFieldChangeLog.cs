namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Audit log entry for custom field changes.
/// </summary>
public class CustomFieldChangeLog
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the custom field descriptor.
    /// </summary>
    public Guid CustomFieldDescriptorId { get; set; }

    /// <summary>
    /// NetSuite field ID for quick lookup.
    /// </summary>
    public required string FieldId { get; set; }

    /// <summary>
    /// Type of change detected.
    /// </summary>
    public required ChangeType ChangeType { get; set; }

    /// <summary>
    /// Previous value (JSON serialized).
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// New value (JSON serialized).
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// When this change was detected.
    /// </summary>
    public DateTime DetectedAt { get; set; }

    /// <summary>
    /// Description of the change.
    /// </summary>
    public string? ChangeDescription { get; set; }

    /// <summary>
    /// Navigation property to the custom field descriptor.
    /// </summary>
    public CustomFieldDescriptor? CustomFieldDescriptor { get; set; }
}
