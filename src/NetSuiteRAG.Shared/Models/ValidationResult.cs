using System.Text.Json.Serialization;

namespace NetSuiteRAG.Shared.Models;

/// <summary>
/// Represents the result of a validation operation.
/// Used to communicate validation success or failure with detailed error information.
/// </summary>
public class ValidationResult
{
    /// <summary>
    /// Indicates whether the validation passed.
    /// True if valid, false if validation errors were found.
    /// </summary>
    [JsonPropertyName("isValid")]
    public required bool IsValid { get; init; }

    /// <summary>
    /// List of validation errors found during validation.
    /// Empty if IsValid is true.
    /// </summary>
    [JsonPropertyName("errors")]
    public required List<ValidationError> Errors { get; init; }

    /// <summary>
    /// List of validation warnings (non-blocking issues).
    /// The validation may still pass with warnings, but they indicate potential problems.
    /// </summary>
    [JsonPropertyName("warnings")]
    public List<ValidationWarning>? Warnings { get; init; }

    /// <summary>
    /// Optional: Additional context about the validation.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; init; }

    /// <summary>
    /// Creates a successful validation result with no errors.
    /// </summary>
    public static ValidationResult Success(string? message = null) => new()
    {
        IsValid = true,
        Errors = [],
        Message = message
    };

    /// <summary>
    /// Creates a failed validation result with errors.
    /// </summary>
    public static ValidationResult Failure(List<ValidationError> errors, string? message = null) => new()
    {
        IsValid = false,
        Errors = errors,
        Message = message
    };

    /// <summary>
    /// Creates a failed validation result with a single error.
    /// </summary>
    public static ValidationResult Failure(ValidationError error, string? message = null) => new()
    {
        IsValid = false,
        Errors = [error],
        Message = message
    };

    /// <summary>
    /// Creates a failed validation result with a simple error message.
    /// </summary>
    public static ValidationResult Failure(string errorMessage) => new()
    {
        IsValid = false,
        Errors = [new ValidationError
        {
            ErrorCode = "VALIDATION_FAILED",
            Message = errorMessage,
            Severity = ValidationSeverity.Error
        }]
    };
}

/// <summary>
/// Represents a single validation error with details about what failed and how to fix it.
/// </summary>
public class ValidationError
{
    /// <summary>
    /// Machine-readable error code (e.g., "REQUIRED_FIELD", "INVALID_TYPE").
    /// Used for programmatic error handling.
    /// </summary>
    [JsonPropertyName("errorCode")]
    public required string ErrorCode { get; init; }

    /// <summary>
    /// Human-readable error message explaining what went wrong.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>
    /// The severity of this validation issue.
    /// </summary>
    [JsonPropertyName("severity")]
    public required ValidationSeverity Severity { get; init; }

    /// <summary>
    /// Optional: The field or property path where the error occurred.
    /// Uses JSON path notation (e.g., "filters[0].field", "recordType").
    /// </summary>
    [JsonPropertyName("fieldPath")]
    public string? FieldPath { get; init; }

    /// <summary>
    /// Optional: The invalid value that caused the error.
    /// </summary>
    [JsonPropertyName("invalidValue")]
    public object? InvalidValue { get; init; }

    /// <summary>
    /// Optional: Suggestion for how to fix this error.
    /// </summary>
    [JsonPropertyName("suggestion")]
    public string? Suggestion { get; init; }

    /// <summary>
    /// Optional: Additional context or details about the error.
    /// </summary>
    [JsonPropertyName("details")]
    public Dictionary<string, object>? Details { get; init; }
}

/// <summary>
/// Represents a validation warning (non-blocking issue).
/// </summary>
public class ValidationWarning
{
    /// <summary>
    /// Machine-readable warning code.
    /// </summary>
    [JsonPropertyName("warningCode")]
    public required string WarningCode { get; init; }

    /// <summary>
    /// Human-readable warning message.
    /// </summary>
    [JsonPropertyName("message")]
    public required string Message { get; init; }

    /// <summary>
    /// Optional: The field or property path where the warning occurred.
    /// </summary>
    [JsonPropertyName("fieldPath")]
    public string? FieldPath { get; init; }

    /// <summary>
    /// Optional: Suggestion for how to address this warning.
    /// </summary>
    [JsonPropertyName("suggestion")]
    public string? Suggestion { get; init; }
}

/// <summary>
/// Represents the severity level of a validation issue.
/// </summary>
[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ValidationSeverity
{
    /// <summary>
    /// Informational message (non-blocking).
    /// </summary>
    Info,

    /// <summary>
    /// Warning (non-blocking, but indicates potential issues).
    /// </summary>
    Warning,

    /// <summary>
    /// Error (blocking, validation fails).
    /// </summary>
    Error,

    /// <summary>
    /// Critical error (blocking, severe issue that must be fixed).
    /// </summary>
    Critical
}
