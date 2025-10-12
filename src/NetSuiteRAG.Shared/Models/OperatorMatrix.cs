namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Defines the operator compatibility matrix for NetSuite field types.
/// Used during query plan validation (Layer 3) to ensure operators are valid for field types.
/// </summary>
public static class OperatorMatrix
{
    /// <summary>
    /// Gets the list of valid operators for a given field type.
    /// </summary>
    /// <param name="fieldType">The field type to check.</param>
    /// <returns>List of valid operators for the field type.</returns>
    public static List<OperatorType> GetValidOperators(FieldType fieldType)
    {
        return fieldType switch
        {
            FieldType.List => [
                OperatorType.AnyOf,
                OperatorType.NoneOf,
                OperatorType.Equals,
                OperatorType.NotEquals,
                OperatorType.IsEmpty,
                OperatorType.IsNotEmpty
            ],

            FieldType.MultiSelect => [
                OperatorType.AnyOf,
                OperatorType.NoneOf,
                OperatorType.IsEmpty,
                OperatorType.IsNotEmpty
            ],

            FieldType.Date => [
                OperatorType.On,
                OperatorType.Before,
                OperatorType.After,
                OperatorType.OnOrBefore,
                OperatorType.OnOrAfter,
                OperatorType.Within,
                OperatorType.NotWithin,
                OperatorType.Between,
                OperatorType.IsEmpty,
                OperatorType.IsNotEmpty
            ],

            FieldType.Number or FieldType.Currency or FieldType.Percent => [
                OperatorType.Equals,
                OperatorType.NotEquals,
                OperatorType.GreaterThan,
                OperatorType.LessThan,
                OperatorType.Between,
                OperatorType.GreaterThanOrEquals,
                OperatorType.LessThanOrEquals,
                OperatorType.IsEmpty,
                OperatorType.IsNotEmpty
            ],

            FieldType.Text or FieldType.Email or FieldType.Phone or FieldType.Url => [
                OperatorType.Contains,
                OperatorType.DoesNotContain,
                OperatorType.StartsWith,
                OperatorType.DoesNotStartWith,
                OperatorType.Is,
                OperatorType.IsNot,
                OperatorType.Equals,
                OperatorType.NotEquals,
                OperatorType.IsEmpty,
                OperatorType.IsNotEmpty
            ],

            FieldType.Checkbox => [
                OperatorType.IsTrue,
                OperatorType.IsFalse,
                OperatorType.Is,
                OperatorType.IsNot,
                OperatorType.Equals,
                OperatorType.NotEquals,
                OperatorType.IsEmpty,
                OperatorType.IsNotEmpty
            ],

            FieldType.Image or FieldType.Document => [
                OperatorType.IsEmpty,
                OperatorType.IsNotEmpty
            ],

            _ => [
                OperatorType.IsEmpty,
                OperatorType.IsNotEmpty
            ]
        };
    }

    /// <summary>
    /// Validates whether an operator is compatible with a field type.
    /// </summary>
    /// <param name="fieldType">The field type.</param>
    /// <param name="operatorType">The operator to validate.</param>
    /// <returns>True if the operator is valid for the field type; otherwise, false.</returns>
    public static bool IsOperatorValid(FieldType fieldType, OperatorType operatorType)
    {
        var validOperators = GetValidOperators(fieldType);
        return validOperators.Contains(operatorType);
    }

    /// <summary>
    /// Gets the expected number of values for an operator.
    /// Used to validate filter value arity.
    /// </summary>
    /// <param name="operatorType">The operator type.</param>
    /// <returns>Expected value count: 0 (no values), 1 (single value), 2 (range), -1 (variable/list).</returns>
    public static int GetExpectedValueCount(OperatorType operatorType)
    {
        return operatorType switch
        {
            OperatorType.IsEmpty or OperatorType.IsNotEmpty or OperatorType.IsTrue or OperatorType.IsFalse => 0,
            OperatorType.Between or OperatorType.Within or OperatorType.NotWithin => 2,
            OperatorType.AnyOf or OperatorType.NoneOf => -1, // Variable count
            _ => 1
        };
    }

    /// <summary>
    /// Gets a human-readable description of the operator.
    /// Used for error messages and documentation.
    /// </summary>
    /// <param name="operatorType">The operator type.</param>
    /// <returns>Human-readable operator description.</returns>
    public static string GetOperatorDescription(OperatorType operatorType)
    {
        return operatorType switch
        {
            OperatorType.AnyOf => "is any of",
            OperatorType.NoneOf => "is none of",
            OperatorType.On => "is on",
            OperatorType.Before => "is before",
            OperatorType.After => "is after",
            OperatorType.Within => "is within range",
            OperatorType.NotWithin => "is not within range",
            OperatorType.NotOn => "is not on",
            OperatorType.OnOrBefore => "is on or before",
            OperatorType.OnOrAfter => "is on or after",
            OperatorType.Equals => "equals",
            OperatorType.NotEquals => "does not equal",
            OperatorType.GreaterThan => "is greater than",
            OperatorType.LessThan => "is less than",
            OperatorType.Between => "is between",
            OperatorType.GreaterThanOrEquals => "is greater than or equal to",
            OperatorType.LessThanOrEquals => "is less than or equal to",
            OperatorType.Contains => "contains",
            OperatorType.DoesNotContain => "does not contain",
            OperatorType.StartsWith => "starts with",
            OperatorType.DoesNotStartWith => "does not start with",
            OperatorType.Is => "is exactly",
            OperatorType.IsNot => "is not",
            OperatorType.IsEmpty => "is empty",
            OperatorType.IsNotEmpty => "is not empty",
            OperatorType.IsTrue => "is true",
            OperatorType.IsFalse => "is false",
            _ => operatorType.ToString()
        };
    }
}
