using System.Diagnostics.CodeAnalysis;

namespace BsSnazzySnippets.Results;

public abstract class ServiceResult
{
    /// <summary>
    /// Create a "Success" service result, with a status code and an optional value.
    /// </summary>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ServiceResult<T> Success<T>(T? value) => new(
        StatusCode: ResultStatusCode.Ok,
        Value: value);
    
    /// <summary>
    /// Create a "Failure" service result, with an exception, error message, status code and value.
    /// </summary>
    /// <param name="exception"></param>
    /// <param name="errorMessage"></param>
    /// <param name="statusCode"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ServiceResult<T> Failure<T>(
        Exception? exception = null,
        string? errorMessage = null,
        ResultStatusCode statusCode = ResultStatusCode.GenericFailure,
        T? value = default)
        => new(
            StatusCode: statusCode,
            Value: value,
            InnerException: exception,
            ErrorMessage: errorMessage
        );

    /// <summary>
    /// Create a "Failure" service result, with an error message, status code and value.
    /// </summary>
    /// <param name="errorMessage"></param>
    /// <param name="statusCode"></param>
    /// <param name="value"></param>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ServiceResult<T> Failure<T>(
        string errorMessage,
        ResultStatusCode statusCode = ResultStatusCode.GenericFailure,
        T? value = default) =>
        Failure(null, errorMessage, statusCode, value);
    
    /// <summary>
    /// Create a "Failure" service result from an existing service result but using a new value.
    /// An error message and/or status code can be specified to override the underlying service result's properties.
    /// </summary>
    /// <param name="serviceResult">Service result to be "copied"</param>
    /// <param name="value">Value of the newly-created service result</param>
    /// <param name="errorMessage">Override the underlying service result's message</param>
    /// <param name="statusCode">Override the underlying service result's status code</param>
    /// <typeparam name="TAlt"></typeparam>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static ServiceResult<T> FailureFromServiceResult<TAlt, T>(
        ServiceResult<TAlt> serviceResult,
        T? value,
        string? errorMessage = null,
        ResultStatusCode? statusCode = null) 
        => Failure<T>(serviceResult.InnerException ?? null, errorMessage ?? serviceResult.ErrorMessage, statusCode ?? serviceResult.StatusCode, value);
}

public record ServiceResult<T>(
    ResultStatusCode StatusCode,
    T? Value,
    Exception? InnerException = null,
    string? ErrorMessage = null
)
{
    public static implicit operator ServiceResult<T>(T? value) => ServiceResult.Success(value);

    public bool IsSuccess => StatusCode == ResultStatusCode.Ok;
    public bool IsFailure => !IsSuccess;

    public T Get()
    {
        return Value ?? throw new ArgumentNullException(nameof(Value));
    }

    public bool TryGet([MaybeNullWhen(false)] out T value)
    {
        value = Value ?? default;
        return Value is not null;
    }

    public ServiceResult<TAlt> PassThroughFail<TAlt>(
        TAlt? value,
        string? errorMessage = null,
        ResultStatusCode? statusCode = null)
    {
        return ServiceResult.FailureFromServiceResult(this, value, errorMessage, statusCode);
    }
}