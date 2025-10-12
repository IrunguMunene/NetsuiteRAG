using NetSuiteRAG.Api.Services.Interfaces;
using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Implementations;

/// <summary>
/// Implements comprehensive schema validation for SavedSearchPlan objects.
/// Validates:
/// - Layer 1: Structure, required fields, type correctness
/// - Layer 2: NetSuite schema (fields exist, joins valid)
/// - Layer 3: Operator compatibility with field types
/// </summary>
public class SchemaValidationService(
    IFieldDictionaryService fieldDictionaryService,
    ILogger<SchemaValidationService> logger) : ISchemaValidationService
{
    /// <summary>
    /// Validates the complete schema of a SavedSearchPlan.
    /// Performs structural, semantic, and compatibility validation.
    /// </summary>
    public ValidationResult ValidateSchema(SavedSearchPlan plan)
    {
        logger.LogInformation("Starting comprehensive schema validation for SavedSearchPlan with recordType: {RecordType}", plan.RecordType);

        var errors = new List<ValidationError>();
        var warnings = new List<ValidationWarning>();

        // Layer 1: Structure validation
        errors.AddRange(ValidateStructure(plan));

        // If structure validation failed, don't proceed to deeper validation
        if (errors.Count > 0)
        {
            logger.LogWarning("Structure validation failed with {ErrorCount} errors", errors.Count);
            return ValidationResult.Failure(errors, "Structure validation failed");
        }

        // Layer 2 & 3: Semantic validation (async operations run synchronously here)
        var semanticErrors = ValidateSemanticAsync(plan).GetAwaiter().GetResult();
        errors.AddRange(semanticErrors);

        if (errors.Count > 0)
        {
            logger.LogWarning("Schema validation failed with {ErrorCount} errors", errors.Count);
            return ValidationResult.Failure(errors, "Schema validation failed");
        }

        logger.LogInformation("Schema validation succeeded");
        return ValidationResult.Success("Schema validation passed");
    }

    /// <summary>
    /// Validates basic structure and required fields (Layer 1).
    /// </summary>
    private List<ValidationError> ValidateStructure(SavedSearchPlan plan)
    {
        var errors = new List<ValidationError>();

        // Validate record type
        var recordTypeError = ValidateRecordType(plan.RecordType);
        if (recordTypeError != null)
        {
            errors.Add(recordTypeError);
        }

        // Validate filters collection exists
        if (plan.Filters == null)
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "REQUIRED_FIELD",
                Message = "Filters collection is required",
                Severity = ValidationSeverity.Error,
                FieldPath = "filters",
                Suggestion = "Provide a filters array (can be empty if no filters needed)"
            });
        }
        else
        {
            for (int i = 0; i < plan.Filters.Count; i++)
            {
                errors.AddRange(ValidateFilter(plan.Filters[i], i));
            }
        }

        // Validate columns collection exists and has at least one column
        if (plan.Columns == null)
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "REQUIRED_FIELD",
                Message = "Columns collection is required",
                Severity = ValidationSeverity.Error,
                FieldPath = "columns",
                Suggestion = "Provide a columns array with at least one column"
            });
        }
        else if (plan.Columns.Count == 0)
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "EMPTY_COLLECTION",
                Message = "At least one column is required",
                Severity = ValidationSeverity.Error,
                FieldPath = "columns",
                Suggestion = "Add at least one column to display in results"
            });
        }
        else
        {
            for (int i = 0; i < plan.Columns.Count; i++)
            {
                errors.AddRange(ValidateColumn(plan.Columns[i], i));
            }
        }

        // Validate sorts if present
        if (plan.Sorts != null)
        {
            for (int i = 0; i < plan.Sorts.Count; i++)
            {
                errors.AddRange(ValidateSort(plan.Sorts[i], i));
            }
        }

        // Validate maxResults if present
        if (plan.MaxResults.HasValue && plan.MaxResults.Value <= 0)
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "INVALID_VALUE",
                Message = "MaxResults must be greater than 0",
                Severity = ValidationSeverity.Error,
                FieldPath = "maxResults",
                InvalidValue = plan.MaxResults.Value,
                Suggestion = "Provide a positive integer or omit for no limit"
            });
        }

        return errors;
    }

    /// <summary>
    /// Validates semantic correctness (Layer 2 and 3): fields exist, operators compatible.
    /// </summary>
    private async Task<List<ValidationError>> ValidateSemanticAsync(SavedSearchPlan plan)
    {
        var errors = new List<ValidationError>();

        // Get all fields for the record type
        var recordTypeFields = await fieldDictionaryService.GetFieldsForRecordTypeAsync(plan.RecordType);
        if (recordTypeFields == null || recordTypeFields.Count == 0)
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "INVALID_RECORD_TYPE",
                Message = $"Record type '{plan.RecordType}' is not valid or has no accessible fields",
                Severity = ValidationSeverity.Error,
                FieldPath = "recordType",
                InvalidValue = plan.RecordType,
                Suggestion = "Use a valid NetSuite record type (e.g., 'transaction', 'customer', 'item')"
            });
            return errors; // Can't proceed without valid record type
        }

        // Validate filters reference valid fields with compatible operators
        for (int i = 0; i < plan.Filters.Count; i++)
        {
            var filter = plan.Filters[i];
            var fieldErrors = await ValidateFieldAndOperatorAsync(
                plan.RecordType,
                filter.Field,
                filter.Operator,
                filter.Join,
                $"filters[{i}]",
                recordTypeFields);
            errors.AddRange(fieldErrors);
        }

        // Validate columns reference valid fields
        for (int i = 0; i < plan.Columns.Count; i++)
        {
            var column = plan.Columns[i];
            var fieldErrors = await ValidateColumnFieldAsync(
                plan.RecordType,
                column.Field,
                column.Join,
                column.Summary,
                $"columns[{i}]",
                recordTypeFields);
            errors.AddRange(fieldErrors);
        }

        // Validate sorts reference fields that exist in columns
        if (plan.Sorts != null)
        {
            for (int i = 0; i < plan.Sorts.Count; i++)
            {
                var sort = plan.Sorts[i];
                var columnExists = plan.Columns.Any(c => c.Field == sort.Field);
                if (!columnExists)
                {
                    errors.Add(new ValidationError
                    {
                        ErrorCode = "INVALID_SORT_FIELD",
                        Message = $"Sort field '{sort.Field}' must exist in the columns list",
                        Severity = ValidationSeverity.Error,
                        FieldPath = $"sorts[{i}].field",
                        InvalidValue = sort.Field,
                        Suggestion = $"Add '{sort.Field}' to the columns list or use a different sort field"
                    });
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates that a field exists and the operator is compatible (Layer 2 and 3).
    /// </summary>
    private async Task<List<ValidationError>> ValidateFieldAndOperatorAsync(
        string recordType,
        string fieldId,
        OperatorType operatorType,
        string? join,
        string fieldPath,
        List<FieldDefinition> recordTypeFields)
    {
        var errors = new List<ValidationError>();

        // If join is specified, get fields for the join
        List<FieldDefinition> fieldsToSearch = recordTypeFields;
        if (!string.IsNullOrWhiteSpace(join))
        {
            var joinFields = await fieldDictionaryService.GetFieldsForJoinAsync(recordType, join);
            if (joinFields == null || joinFields.Count == 0)
            {
                errors.Add(new ValidationError
                {
                    ErrorCode = "INVALID_JOIN",
                    Message = $"Join '{join}' is not valid for record type '{recordType}'",
                    Severity = ValidationSeverity.Error,
                    FieldPath = $"{fieldPath}.join",
                    InvalidValue = join,
                    Suggestion = "Remove the join or use a valid join name for this record type"
                });
                return errors; // Can't proceed without valid join
            }
            fieldsToSearch = joinFields;
        }

        // Check if field exists
        var field = fieldsToSearch.FirstOrDefault(f => string.Equals(f.FieldId, fieldId, StringComparison.OrdinalIgnoreCase));
        if (field == null)
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "INVALID_FIELD",
                Message = $"Field '{fieldId}' does not exist for record type '{recordType}'" +
                         (string.IsNullOrWhiteSpace(join) ? "" : $" with join '{join}'"),
                Severity = ValidationSeverity.Error,
                FieldPath = $"{fieldPath}.field",
                InvalidValue = fieldId,
                Suggestion = "Use a valid field identifier for this record type"
            });
            return errors;
        }

        // Check operator compatibility with field type (Layer 3)
        if (!OperatorMatrix.IsOperatorValid(field.FieldType, operatorType))
        {
            var validOperators = OperatorMatrix.GetValidOperators(field.FieldType);
            var validOperatorsStr = string.Join(", ", validOperators.Select(o => o.ToString()));

            errors.Add(new ValidationError
            {
                ErrorCode = "INCOMPATIBLE_OPERATOR",
                Message = $"Operator '{operatorType}' is not compatible with field type '{field.FieldType}' for field '{fieldId}'",
                Severity = ValidationSeverity.Error,
                FieldPath = $"{fieldPath}.operator",
                InvalidValue = operatorType,
                Suggestion = $"Use one of these operators for {field.FieldType} fields: {validOperatorsStr}",
                Details = new Dictionary<string, object>
                {
                    ["fieldType"] = field.FieldType.ToString(),
                    ["validOperators"] = validOperators.Select(o => o.ToString()).ToList()
                }
            });
        }

        return errors;
    }

    /// <summary>
    /// Validates that a column field exists (Layer 2).
    /// </summary>
    private async Task<List<ValidationError>> ValidateColumnFieldAsync(
        string recordType,
        string fieldId,
        string? join,
        SummaryType? summary,
        string fieldPath,
        List<FieldDefinition> recordTypeFields)
    {
        var errors = new List<ValidationError>();

        // If join is specified, get fields for the join
        List<FieldDefinition> fieldsToSearch = recordTypeFields;
        if (!string.IsNullOrWhiteSpace(join))
        {
            var joinFields = await fieldDictionaryService.GetFieldsForJoinAsync(recordType, join);
            if (joinFields == null || joinFields.Count == 0)
            {
                errors.Add(new ValidationError
                {
                    ErrorCode = "INVALID_JOIN",
                    Message = $"Join '{join}' is not valid for record type '{recordType}'",
                    Severity = ValidationSeverity.Error,
                    FieldPath = $"{fieldPath}.join",
                    InvalidValue = join,
                    Suggestion = "Remove the join or use a valid join name for this record type"
                });
                return errors;
            }
            fieldsToSearch = joinFields;
        }

        // Check if field exists
        var field = fieldsToSearch.FirstOrDefault(f => string.Equals(f.FieldId, fieldId, StringComparison.OrdinalIgnoreCase));
        if (field == null)
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "INVALID_FIELD",
                Message = $"Field '{fieldId}' does not exist for record type '{recordType}'" +
                         (string.IsNullOrWhiteSpace(join) ? "" : $" with join '{join}'"),
                Severity = ValidationSeverity.Error,
                FieldPath = $"{fieldPath}.field",
                InvalidValue = fieldId,
                Suggestion = "Use a valid field identifier for this record type"
            });
            return errors;
        }

        // Validate summary type is compatible with field type
        if (summary.HasValue && summary.Value != SummaryType.None && summary.Value != SummaryType.Group)
        {
            var numericSummaries = new[] { SummaryType.Sum, SummaryType.Average };
            var isNumericSummary = numericSummaries.Contains(summary.Value);

            if (isNumericSummary)
            {
                var numericTypes = new[] { FieldType.Number, FieldType.Currency, FieldType.Percent };
                if (!numericTypes.Contains(field.FieldType))
                {
                    errors.Add(new ValidationError
                    {
                        ErrorCode = "INCOMPATIBLE_SUMMARY",
                        Message = $"Summary type '{summary.Value}' is not compatible with field type '{field.FieldType}' for field '{fieldId}'",
                        Severity = ValidationSeverity.Error,
                        FieldPath = $"{fieldPath}.summary",
                        InvalidValue = summary.Value,
                        Suggestion = $"Use Count, Group, or remove summary for {field.FieldType} fields"
                    });
                }
            }
        }

        return errors;
    }

    /// <summary>
    /// Validates a single filter object (Layer 1 structure).
    /// </summary>
    public List<ValidationError> ValidateFilter(Filter filter, int index)
    {
        var errors = new List<ValidationError>();

        // Validate field is not null or empty
        if (string.IsNullOrWhiteSpace(filter.Field))
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "REQUIRED_FIELD",
                Message = "Filter field is required",
                Severity = ValidationSeverity.Error,
                FieldPath = $"filters[{index}].field",
                Suggestion = "Provide a valid field identifier (e.g., 'trandate', 'entity')"
            });
        }

        // Validate operator is a valid enum value
        if (!Enum.IsDefined(typeof(OperatorType), filter.Operator))
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "INVALID_ENUM",
                Message = $"Invalid operator: {filter.Operator}",
                Severity = ValidationSeverity.Error,
                FieldPath = $"filters[{index}].operator",
                InvalidValue = filter.Operator,
                Suggestion = "Use a valid OperatorType enum value"
            });
        }

        // Validate value based on operator requirements
        var valueError = ValidateFilterValue(filter, index);
        if (valueError != null)
        {
            errors.Add(valueError);
        }

        return errors;
    }

    /// <summary>
    /// Validates a single column object (Layer 1 structure).
    /// </summary>
    public List<ValidationError> ValidateColumn(Column column, int index)
    {
        var errors = new List<ValidationError>();

        // Validate field is not null or empty
        if (string.IsNullOrWhiteSpace(column.Field))
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "REQUIRED_FIELD",
                Message = "Column field is required",
                Severity = ValidationSeverity.Error,
                FieldPath = $"columns[{index}].field",
                Suggestion = "Provide a valid field identifier (e.g., 'trandate', 'entity')"
            });
        }

        // Validate label is not null or empty
        if (string.IsNullOrWhiteSpace(column.Label))
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "REQUIRED_FIELD",
                Message = "Column label is required",
                Severity = ValidationSeverity.Error,
                FieldPath = $"columns[{index}].label",
                Suggestion = "Provide a human-readable label for this column"
            });
        }

        // Validate summary type if present
        if (column.Summary.HasValue && !Enum.IsDefined(typeof(SummaryType), column.Summary.Value))
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "INVALID_ENUM",
                Message = $"Invalid summary type: {column.Summary.Value}",
                Severity = ValidationSeverity.Error,
                FieldPath = $"columns[{index}].summary",
                InvalidValue = column.Summary.Value,
                Suggestion = "Use a valid SummaryType enum value or omit for no aggregation"
            });
        }

        return errors;
    }

    /// <summary>
    /// Validates a single sort object (Layer 1 structure).
    /// </summary>
    public List<ValidationError> ValidateSort(Sort sort, int index)
    {
        var errors = new List<ValidationError>();

        // Validate field is not null or empty
        if (string.IsNullOrWhiteSpace(sort.Field))
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "REQUIRED_FIELD",
                Message = "Sort field is required",
                Severity = ValidationSeverity.Error,
                FieldPath = $"sorts[{index}].field",
                Suggestion = "Provide a valid field identifier from the columns list"
            });
        }

        // Validate direction is a valid enum value
        if (!Enum.IsDefined(typeof(SortDirection), sort.Direction))
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "INVALID_ENUM",
                Message = $"Invalid sort direction: {sort.Direction}",
                Severity = ValidationSeverity.Error,
                FieldPath = $"sorts[{index}].direction",
                InvalidValue = sort.Direction,
                Suggestion = "Use Ascending or Descending"
            });
        }

        // Validate priority if present
        if (sort.Priority.HasValue && sort.Priority.Value <= 0)
        {
            errors.Add(new ValidationError
            {
                ErrorCode = "INVALID_VALUE",
                Message = "Sort priority must be greater than 0",
                Severity = ValidationSeverity.Error,
                FieldPath = $"sorts[{index}].priority",
                InvalidValue = sort.Priority.Value,
                Suggestion = "Provide a positive integer for priority (1 = highest)"
            });
        }

        return errors;
    }

    /// <summary>
    /// Validates the record type is not null or empty.
    /// </summary>
    public ValidationError? ValidateRecordType(string recordType)
    {
        if (string.IsNullOrWhiteSpace(recordType))
        {
            return new ValidationError
            {
                ErrorCode = "REQUIRED_FIELD",
                Message = "RecordType is required",
                Severity = ValidationSeverity.Error,
                FieldPath = "recordType",
                Suggestion = "Provide a valid NetSuite record type (e.g., 'transaction', 'customer')"
            };
        }

        return null;
    }

    /// <summary>
    /// Validates that filter value matches the expected arity for the operator.
    /// </summary>
    public ValidationError? ValidateFilterValue(Filter filter, int index)
    {
        var expectedCount = OperatorMatrix.GetExpectedValueCount(filter.Operator);

        // Operators that don't need values (IsEmpty, IsNotEmpty)
        if (expectedCount == 0)
        {
            // Value should be null or empty
            return null;
        }

        // Value is required for all other operators
        if (filter.Value == null)
        {
            return new ValidationError
            {
                ErrorCode = "REQUIRED_FIELD",
                Message = "Filter value is required for this operator",
                Severity = ValidationSeverity.Error,
                FieldPath = $"filters[{index}].value",
                Suggestion = $"Provide {(expectedCount == -1 ? "one or more values" : expectedCount == 1 ? "a single value" : $"{expectedCount} values")} for the {filter.Operator} operator"
            };
        }

        // Variable count operators (AnyOf, NoneOf) - expect array
        if (expectedCount == -1)
        {
            if (filter.Value is not System.Collections.IEnumerable enumerable || filter.Value is string)
            {
                return new ValidationError
                {
                    ErrorCode = "INVALID_VALUE_TYPE",
                    Message = $"Operator {filter.Operator} requires an array of values",
                    Severity = ValidationSeverity.Error,
                    FieldPath = $"filters[{index}].value",
                    InvalidValue = filter.Value,
                    Suggestion = "Provide an array of values (e.g., [\"value1\", \"value2\"])"
                };
            }

            var list = System.Linq.Enumerable.ToList((dynamic)enumerable);
            if (list.Count == 0)
            {
                return new ValidationError
                {
                    ErrorCode = "EMPTY_VALUE",
                    Message = $"Operator {filter.Operator} requires at least one value",
                    Severity = ValidationSeverity.Error,
                    FieldPath = $"filters[{index}].value",
                    Suggestion = "Provide at least one value in the array"
                };
            }
        }
        // Range operators (Between, Within) - expect exactly 2 values
        else if (expectedCount == 2)
        {
            if (filter.Value is not System.Collections.IEnumerable enumerable || filter.Value is string)
            {
                return new ValidationError
                {
                    ErrorCode = "INVALID_VALUE_TYPE",
                    Message = $"Operator {filter.Operator} requires an array of exactly 2 values (start and end)",
                    Severity = ValidationSeverity.Error,
                    FieldPath = $"filters[{index}].value",
                    InvalidValue = filter.Value,
                    Suggestion = "Provide an array with 2 values (e.g., [\"2024-01-01\", \"2024-12-31\"])"
                };
            }

            var list = System.Linq.Enumerable.ToList((dynamic)enumerable);
            if (list.Count != 2)
            {
                return new ValidationError
                {
                    ErrorCode = "INVALID_VALUE_COUNT",
                    Message = $"Operator {filter.Operator} requires exactly 2 values, but {list.Count} provided",
                    Severity = ValidationSeverity.Error,
                    FieldPath = $"filters[{index}].value",
                    InvalidValue = filter.Value,
                    Suggestion = "Provide exactly 2 values (start and end of range)"
                };
            }
        }
        // Single value operators - allow scalar or single-element array
        else if (expectedCount == 1)
        {
            if (filter.Value is System.Collections.IEnumerable enumerable && filter.Value is not string)
            {
                var list = System.Linq.Enumerable.ToList((dynamic)enumerable);
                if (list.Count > 1)
                {
                    return new ValidationError
                    {
                        ErrorCode = "INVALID_VALUE_TYPE",
                        Message = $"Operator {filter.Operator} requires a single value, not an array",
                        Severity = ValidationSeverity.Error,
                        FieldPath = $"filters[{index}].value",
                        InvalidValue = filter.Value,
                        Suggestion = "Provide a single value instead of an array"
                    };
                }
            }
        }

        return null;
    }
}
