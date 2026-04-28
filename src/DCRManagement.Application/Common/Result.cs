namespace DCRManagement.Application.Common;

/// <summary>
/// Discriminated union result type — eliminates exception-as-control-flow anti-pattern.
/// Services return Result{T} instead of throwing for expected failures (validation, not found).
/// Only truly unexpected errors (DB down, network) bubble up as exceptions.
/// </summary>
public class Result<T>
{
    public bool IsSuccess { get; private init; }
    public T? Value { get; private init; }
    public string? ErrorMessage { get; private init; }
    public string? ErrorCode { get; private init; }

    private Result() { }

    public static Result<T> Success(T value) => new()
    {
        IsSuccess = true,
        Value = value
    };

    public static Result<T> Failure(string errorMessage, string? errorCode = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };

    // Implicit conversion so callers can write: return value; instead of Result<T>.Success(value)
    public static implicit operator Result<T>(T value) => Success(value);
}

// Non-generic variant for operations that return no value (void-like)
public class Result
{
    public bool IsSuccess { get; private init; }
    public string? ErrorMessage { get; private init; }
    public string? ErrorCode { get; private init; }

    private Result() { }

    public static readonly Result Ok = new() { IsSuccess = true };

    public static Result Success() => Ok;

    public static Result Failure(string errorMessage, string? errorCode = null) => new()
    {
        IsSuccess = false,
        ErrorMessage = errorMessage,
        ErrorCode = errorCode
    };
}