using System.Text.Json.Serialization;

namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents a sort order for NetSuite Saved Search results.
/// Sorts define how the result set is ordered.
/// </summary>
public class Sort
{
    /// <summary>
    /// The field identifier to sort by (e.g., "trandate", "amount").
    /// Must be a valid field included in the columns list.
    /// </summary>
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    /// <summary>
    /// The sort direction (Ascending or Descending).
    /// </summary>
    [JsonPropertyName("direction")]
    public required SortDirection Direction { get; init; }

    /// <summary>
    /// Optional: Sort priority when multiple sorts are specified.
    /// Lower numbers have higher priority (1 = first sort, 2 = second sort, etc.).
    /// </summary>
    [JsonPropertyName("priority")]
    public int? Priority { get; init; }
}
