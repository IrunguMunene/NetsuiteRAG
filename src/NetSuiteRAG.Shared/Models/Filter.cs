using System.Text.Json.Serialization;

namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents a filter condition in a NetSuite Saved Search.
/// Filters define criteria for which records to include in the search results.
/// </summary>
public class Filter
{
    /// <summary>
    /// The field identifier to filter on (e.g., "trandate", "entity", "amount").
    /// Must be a valid field for the search's record type.
    /// </summary>
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    /// <summary>
    /// The filter operator to apply (e.g., Equals, Contains, AnyOf).
    /// Must be compatible with the field's data type.
    /// </summary>
    [JsonPropertyName("operator")]
    public required OperatorType Operator { get; init; }

    /// <summary>
    /// The value(s) to filter against.
    /// Can be a single value or multiple values depending on the operator.
    /// - Single value: "John Smith", "100", "2024-01-01"
    /// - Multiple values: ["Customer", "Prospect"], ["1", "2", "3"]
    /// </summary>
    [JsonPropertyName("value")]
    public required object Value { get; init; }

    /// <summary>
    /// Optional: Join to another record type (e.g., "customer", "item").
    /// Used for cross-record filtering.
    /// </summary>
    [JsonPropertyName("join")]
    public string? Join { get; init; }

    /// <summary>
    /// Optional: Human-readable label for this filter.
    /// Used for display purposes in the UI.
    /// </summary>
    [JsonPropertyName("label")]
    public string? Label { get; init; }
}
