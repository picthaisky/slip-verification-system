namespace SlipVerification.Shared.Results;

/// <summary>
/// Represents the result of an operation
/// </summary>
/// <typeparam name="T">The type of the result data</typeparam>
public class Result<T>
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// Gets the result data
    /// </summary>
    public T? Data { get; init; }
    
    /// <summary>
    /// Gets the error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Gets the collection of error messages
    /// </summary>
    public IEnumerable<string>? Errors { get; init; }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result<T> Success(T data) => new() { IsSuccess = true, Data = data };
    
    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    public static Result<T> Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
    
    /// <summary>
    /// Creates a failed result with multiple error messages
    /// </summary>
    public static Result<T> Failure(IEnumerable<string> errors) => new() { IsSuccess = false, Errors = errors };
}

/// <summary>
/// Represents the result of an operation without data
/// </summary>
public class Result
{
    /// <summary>
    /// Gets a value indicating whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; init; }
    
    /// <summary>
    /// Gets the error message if the operation failed
    /// </summary>
    public string? ErrorMessage { get; init; }
    
    /// <summary>
    /// Gets the collection of error messages
    /// </summary>
    public IEnumerable<string>? Errors { get; init; }
    
    /// <summary>
    /// Creates a successful result
    /// </summary>
    public static Result Success() => new() { IsSuccess = true };
    
    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    public static Result Failure(string errorMessage) => new() { IsSuccess = false, ErrorMessage = errorMessage };
    
    /// <summary>
    /// Creates a failed result with multiple error messages
    /// </summary>
    public static Result Failure(IEnumerable<string> errors) => new() { IsSuccess = false, Errors = errors };
}
