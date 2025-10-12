using System.Text.Json.Serialization;

namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents a column to display in NetSuite Saved Search results.
/// Columns define which fields to include in the result set.
/// </summary>
public class Column
{
    /// <summary>
    /// The field identifier to display (e.g., "trandate", "entity", "amount").
    /// Must be a valid field for the search's record type.
    /// </summary>
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    /// <summary>
    /// Human-readable label for this column.
    /// Displayed as the column header in the results table.
    /// </summary>
    [JsonPropertyName("label")]
    public required string Label { get; init; }

    /// <summary>
    /// Optional: Summary/aggregation type for this column.
    /// Used for grouping and aggregation (Sum, Average, Count, etc.).
    /// Defaults to None (no aggregation).
    /// </summary>
    [JsonPropertyName("summary")]
    public SummaryType? Summary { get; init; }

    /// <summary>
    /// Optional: Join to another record type (e.g., "customer", "item").
    /// Used for cross-record column display.
    /// </summary>
    [JsonPropertyName("join")]
    public string? Join { get; init; }

    /// <summary>
    /// Optional: Function to apply to the field value (e.g., "MONTH", "YEAR").
    /// Used for date transformations and calculations.
    /// </summary>
    [JsonPropertyName("function")]
    public string? Function { get; init; }
}
