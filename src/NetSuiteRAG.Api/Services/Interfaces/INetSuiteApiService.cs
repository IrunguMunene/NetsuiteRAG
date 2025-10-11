using NetSuiteRAG.Shared.Models;

namespace NetSuiteRAG.Api.Services.Interfaces;

/// <summary>
/// Service for interacting with the NetSuite API.
/// </summary>
public interface INetSuiteApiService
{
    /// <summary>
    /// Retrieves all custom fields from NetSuite.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of custom field metadata.</returns>
    Task<Result<List<NetSuiteCustomFieldMetadata>>> GetAllCustomFieldsAsync(
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves custom fields for a specific record type.
    /// </summary>
    /// <param name="recordType">Record type (e.g., "transaction", "customer").</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>List of custom field metadata for the record type.</returns>
    Task<Result<List<NetSuiteCustomFieldMetadata>>> GetCustomFieldsByRecordTypeAsync(
        string recordType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Tests the connection to NetSuite.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>True if connection is successful; otherwise, false.</returns>
    Task<Result<bool>> TestConnectionAsync(
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Represents the result of an operation with success/failure state.
/// </summary>
/// <typeparam name="T">The type of the result value.</typeparam>
public record Result<T>(
    bool IsSuccess,
    T? Value,
    string? Error
)
{
    /// <summary>
    /// Creates a successful result.
    /// </summary>
    /// <param name="value">The result value.</param>
    /// <returns>A successful result.</returns>
    public static Result<T> Success(T value) => new(true, value, null);

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    /// <param name="error">The error message.</param>
    /// <returns>A failed result.</returns>
    public static Result<T> Failure(string error) => new(false, default, error);
}
