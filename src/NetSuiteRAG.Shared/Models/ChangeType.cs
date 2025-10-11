namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Type of change detected for a custom field.
/// </summary>
public enum ChangeType
{
    /// <summary>
    /// New custom field discovered.
    /// </summary>
    New,

    /// <summary>
    /// Existing field metadata updated.
    /// </summary>
    Updated,

    /// <summary>
    /// Field no longer exists in NetSuite.
    /// </summary>
    Deleted,

    /// <summary>
    /// Field not seen in 90+ days.
    /// </summary>
    Stale
}
