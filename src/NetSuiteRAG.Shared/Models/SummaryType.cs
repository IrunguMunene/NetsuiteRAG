namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents the summary/aggregation type for NetSuite Saved Search columns.
/// Used for grouping and aggregation in search results.
/// </summary>
public enum SummaryType
{
    /// <summary>
    /// No aggregation (regular column display).
    /// </summary>
    None,

    /// <summary>
    /// Group by this column.
    /// </summary>
    Group,

    /// <summary>
    /// Sum of values in this column.
    /// Valid for: Number, Currency, Percent
    /// </summary>
    Sum,

    /// <summary>
    /// Average of values in this column.
    /// Valid for: Number, Currency, Percent
    /// </summary>
    Average,

    /// <summary>
    /// Count of records.
    /// Valid for: All types
    /// </summary>
    Count,

    /// <summary>
    /// Minimum value in this column.
    /// Valid for: Number, Currency, Percent, Date
    /// </summary>
    Minimum,

    /// <summary>
    /// Maximum value in this column.
    /// Valid for: Number, Currency, Percent, Date
    /// </summary>
    Maximum
}
