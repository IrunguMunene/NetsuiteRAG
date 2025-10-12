using System.Text.Json.Serialization;

namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents ambiguous field values that require user disambiguation.
/// When the LLM cannot determine a specific entity, period, or subsidiary,
/// it marks these fields as ambiguous and returns options for the user to choose from.
/// </summary>
public class AmbiguousFields
{
    /// <summary>
    /// Optional: Ambiguous entity reference (e.g., customer, vendor, employee).
    /// Contains possible matches when multiple entities have similar names.
    /// </summary>
    [JsonPropertyName("entity")]
    public AmbiguousEntity? Entity { get; init; }

    /// <summary>
    /// Optional: Ambiguous accounting period reference.
    /// Contains possible matches when the time reference is unclear.
    /// </summary>
    [JsonPropertyName("period")]
    public AmbiguousPeriod? Period { get; init; }

    /// <summary>
    /// Optional: Ambiguous subsidiary reference.
    /// Contains possible matches when multiple subsidiaries could apply.
    /// </summary>
    [JsonPropertyName("subsidiary")]
    public AmbiguousSubsidiary? Subsidiary { get; init; }

    /// <summary>
    /// Optional: Ambiguous location reference.
    /// Contains possible matches when multiple locations could apply.
    /// </summary>
    [JsonPropertyName("location")]
    public AmbiguousLocation? Location { get; init; }
}

/// <summary>
/// Represents an ambiguous entity reference requiring user selection.
/// </summary>
public class AmbiguousEntity
{
    /// <summary>
    /// The field name that has ambiguous values (e.g., "entity", "customer").
    /// </summary>
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    /// <summary>
    /// The user's natural language reference (e.g., "John Smith").
    /// </summary>
    [JsonPropertyName("userInput")]
    public required string UserInput { get; init; }

    /// <summary>
    /// List of possible entity matches.
    /// </summary>
    [JsonPropertyName("options")]
    public required List<EntityOption> Options { get; init; }
}

/// <summary>
/// Represents a possible entity match option.
/// </summary>
public class EntityOption
{
    /// <summary>
    /// NetSuite internal ID for this entity.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Display name for this entity.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Entity type (e.g., "Customer", "Vendor", "Employee").
    /// </summary>
    [JsonPropertyName("type")]
    public required string Type { get; init; }

    /// <summary>
    /// Optional: Additional context to help user distinguish (e.g., email, phone).
    /// </summary>
    [JsonPropertyName("context")]
    public string? Context { get; init; }
}

/// <summary>
/// Represents an ambiguous period reference requiring user selection.
/// </summary>
public class AmbiguousPeriod
{
    /// <summary>
    /// The field name that has ambiguous values (e.g., "postingperiod").
    /// </summary>
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    /// <summary>
    /// The user's natural language reference (e.g., "last quarter").
    /// </summary>
    [JsonPropertyName("userInput")]
    public required string UserInput { get; init; }

    /// <summary>
    /// List of possible period matches.
    /// </summary>
    [JsonPropertyName("options")]
    public required List<PeriodOption> Options { get; init; }
}

/// <summary>
/// Represents a possible period match option.
/// </summary>
public class PeriodOption
{
    /// <summary>
    /// NetSuite internal ID for this period.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Display name for this period (e.g., "Q1 2024", "January 2024").
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Start date of this period.
    /// </summary>
    [JsonPropertyName("startDate")]
    public required string StartDate { get; init; }

    /// <summary>
    /// End date of this period.
    /// </summary>
    [JsonPropertyName("endDate")]
    public required string EndDate { get; init; }
}

/// <summary>
/// Represents an ambiguous subsidiary reference requiring user selection.
/// </summary>
public class AmbiguousSubsidiary
{
    /// <summary>
    /// The field name that has ambiguous values (e.g., "subsidiary").
    /// </summary>
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    /// <summary>
    /// The user's natural language reference (e.g., "US operations").
    /// </summary>
    [JsonPropertyName("userInput")]
    public required string UserInput { get; init; }

    /// <summary>
    /// List of possible subsidiary matches.
    /// </summary>
    [JsonPropertyName("options")]
    public required List<SubsidiaryOption> Options { get; init; }
}

/// <summary>
/// Represents a possible subsidiary match option.
/// </summary>
public class SubsidiaryOption
{
    /// <summary>
    /// NetSuite internal ID for this subsidiary.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Display name for this subsidiary.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Optional: Country or region for this subsidiary.
    /// </summary>
    [JsonPropertyName("region")]
    public string? Region { get; init; }
}

/// <summary>
/// Represents an ambiguous location reference requiring user selection.
/// </summary>
public class AmbiguousLocation
{
    /// <summary>
    /// The field name that has ambiguous values (e.g., "location").
    /// </summary>
    [JsonPropertyName("field")]
    public required string Field { get; init; }

    /// <summary>
    /// The user's natural language reference (e.g., "warehouse").
    /// </summary>
    [JsonPropertyName("userInput")]
    public required string UserInput { get; init; }

    /// <summary>
    /// List of possible location matches.
    /// </summary>
    [JsonPropertyName("options")]
    public required List<LocationOption> Options { get; init; }
}

/// <summary>
/// Represents a possible location match option.
/// </summary>
public class LocationOption
{
    /// <summary>
    /// NetSuite internal ID for this location.
    /// </summary>
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    /// <summary>
    /// Display name for this location.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    /// <summary>
    /// Optional: Address or additional context for this location.
    /// </summary>
    [JsonPropertyName("address")]
    public string? Address { get; init; }
}
