using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Interfaces;

/// <summary>
/// Provides schema validation services for SavedSearchPlan objects.
/// This is Layer 1 of the 5-layer validation pipeline.
/// Validates structural integrity, required fields, and type correctness.
/// </summary>
public interface ISchemaValidationService
{
    /// <summary>
    /// Validates the schema and structure of a SavedSearchPlan.
    /// Checks for required fields, correct types, and valid enum values.
    /// </summary>
    /// <param name="plan">The SavedSearchPlan to validate.</param>
    /// <returns>ValidationResult indicating success or containing detailed error information.</returns>
    ValidationResult ValidateSchema(SavedSearchPlan plan);

    /// <summary>
    /// Validates a single filter object.
    /// </summary>
    /// <param name="filter">The filter to validate.</param>
    /// <param name="index">The index of the filter in the filters array (for error reporting).</param>
    /// <returns>List of validation errors found (empty if valid).</returns>
    List<ValidationError> ValidateFilter(Filter filter, int index);

    /// <summary>
    /// Validates a single column object.
    /// </summary>
    /// <param name="column">The column to validate.</param>
    /// <param name="index">The index of the column in the columns array (for error reporting).</param>
    /// <returns>List of validation errors found (empty if valid).</returns>
    List<ValidationError> ValidateColumn(Column column, int index);

    /// <summary>
    /// Validates a single sort object.
    /// </summary>
    /// <param name="sort">The sort to validate.</param>
    /// <param name="index">The index of the sort in the sorts array (for error reporting).</param>
    /// <returns>List of validation errors found (empty if valid).</returns>
    List<ValidationError> ValidateSort(Sort sort, int index);

    /// <summary>
    /// Validates the record type is not null or empty.
    /// </summary>
    /// <param name="recordType">The record type to validate.</param>
    /// <returns>ValidationError if invalid, null if valid.</returns>
    ValidationError? ValidateRecordType(string recordType);

    /// <summary>
    /// Validates that filter value matches the expected type for the operator.
    /// </summary>
    /// <param name="filter">The filter containing the value to validate.</param>
    /// <param name="index">The index of the filter (for error reporting).</param>
    /// <returns>ValidationError if invalid, null if valid.</returns>
    ValidationError? ValidateFilterValue(Filter filter, int index);
}
