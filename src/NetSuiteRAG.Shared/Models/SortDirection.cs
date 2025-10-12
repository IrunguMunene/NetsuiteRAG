namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents the sort direction for NetSuite Saved Search columns.
/// </summary>
public enum SortDirection
{
    /// <summary>
    /// Sort in ascending order (A-Z, 0-9, oldest to newest).
    /// </summary>
    Ascending,

    /// <summary>
    /// Sort in descending order (Z-A, 9-0, newest to oldest).
    /// </summary>
    Descending
}
