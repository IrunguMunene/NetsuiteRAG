namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents the data type of a NetSuite field.
/// Used for operator compatibility validation and display formatting.
/// </summary>
public enum FieldType
{
    /// <summary>
    /// Text field (string values).
    /// </summary>
    Text,

    /// <summary>
    /// Numeric field (integer or decimal values).
    /// </summary>
    Number,

    /// <summary>
    /// Date field (date or datetime values).
    /// </summary>
    Date,

    /// <summary>
    /// List/Select field (enumerated values, references).
    /// </summary>
    List,

    /// <summary>
    /// Checkbox/Boolean field (true/false values).
    /// </summary>
    Checkbox,

    /// <summary>
    /// Currency field (monetary values with currency code).
    /// </summary>
    Currency,

    /// <summary>
    /// Percent field (percentage values).
    /// </summary>
    Percent,

    /// <summary>
    /// Email field (email addresses).
    /// </summary>
    Email,

    /// <summary>
    /// Phone field (phone numbers).
    /// </summary>
    Phone,

    /// <summary>
    /// URL field (web addresses).
    /// </summary>
    Url,

    /// <summary>
    /// Multi-select field (multiple enumerated values).
    /// </summary>
    MultiSelect,

    /// <summary>
    /// Image field (image references).
    /// </summary>
    Image,

    /// <summary>
    /// Document field (file references).
    /// </summary>
    Document
}
