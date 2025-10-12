namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents the filter operators available in NetSuite Saved Search.
/// Operators are validated against field types using the operator compatibility matrix.
/// </summary>
public enum OperatorType
{
    // List operators (for List, MultiSelect fields)

    /// <summary>
    /// Matches any of the specified values (OR logic).
    /// Valid for: List, MultiSelect
    /// </summary>
    AnyOf,

    /// <summary>
    /// Excludes all specified values (NOT IN logic).
    /// Valid for: List, MultiSelect
    /// </summary>
    NoneOf,

    // Date operators

    /// <summary>
    /// Exact date match.
    /// Valid for: Date
    /// </summary>
    On,

    /// <summary>
    /// Date is before specified date.
    /// Valid for: Date
    /// </summary>
    Before,

    /// <summary>
    /// Date is after specified date.
    /// Valid for: Date
    /// </summary>
    After,

    /// <summary>
    /// Date is within specified range (inclusive).
    /// Valid for: Date
    /// </summary>
    Within,

    /// <summary>
    /// Date is not within specified range (exclusive).
    /// Valid for: Date
    /// </summary>
    NotWithin,

    /// <summary>
    /// Date is not on specified date.
    /// Valid for: Date
    /// </summary>
    NotOn,

    /// <summary>
    /// Date is on or before specified date.
    /// Valid for: Date
    /// </summary>
    OnOrBefore,

    /// <summary>
    /// Date is on or after specified date.
    /// Valid for: Date
    /// </summary>
    OnOrAfter,

    // Number operators

    /// <summary>
    /// Exact equality.
    /// Valid for: Number, Currency, Percent, Text
    /// </summary>
    Equals,

    /// <summary>
    /// Greater than specified value.
    /// Valid for: Number, Currency, Percent
    /// </summary>
    GreaterThan,

    /// <summary>
    /// Less than specified value.
    /// Valid for: Number, Currency, Percent
    /// </summary>
    LessThan,

    /// <summary>
    /// Value is between specified range (inclusive).
    /// Valid for: Number, Currency, Percent
    /// </summary>
    Between,

    /// <summary>
    /// Greater than or equal to specified value.
    /// Valid for: Number, Currency, Percent
    /// </summary>
    GreaterThanOrEquals,

    /// <summary>
    /// Less than or equal to specified value.
    /// Valid for: Number, Currency, Percent
    /// </summary>
    LessThanOrEquals,

    /// <summary>
    /// Not equal to specified value.
    /// Valid for: Number, Currency, Percent, Text
    /// </summary>
    NotEquals,

    // Text operators

    /// <summary>
    /// Text contains specified substring (case-insensitive).
    /// Valid for: Text, Email, Phone, Url
    /// </summary>
    Contains,

    /// <summary>
    /// Text does not contain specified substring.
    /// Valid for: Text, Email, Phone, Url
    /// </summary>
    DoesNotContain,

    /// <summary>
    /// Text starts with specified prefix (case-insensitive).
    /// Valid for: Text, Email, Phone, Url
    /// </summary>
    StartsWith,

    /// <summary>
    /// Text does not start with specified prefix.
    /// Valid for: Text, Email, Phone, Url
    /// </summary>
    DoesNotStartWith,

    /// <summary>
    /// Exact text match (case-sensitive).
    /// Valid for: Text, Email, Phone, Url
    /// </summary>
    Is,

    /// <summary>
    /// Not exact text match.
    /// Valid for: Text, Email, Phone, Url
    /// </summary>
    IsNot,

    /// <summary>
    /// Field is empty or null.
    /// Valid for: All types
    /// </summary>
    IsEmpty,

    /// <summary>
    /// Field is not empty or null.
    /// Valid for: All types
    /// </summary>
    IsNotEmpty,

    // Boolean/Checkbox operators

    /// <summary>
    /// Boolean field is true.
    /// Valid for: Checkbox, Boolean
    /// </summary>
    IsTrue,

    /// <summary>
    /// Boolean field is false.
    /// Valid for: Checkbox, Boolean
    /// </summary>
    IsFalse
}
