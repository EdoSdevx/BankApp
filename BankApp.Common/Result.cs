namespace BankApp.BankApp.Common;

public class Result
{
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public int StatusCode { get; init; }
    public ResultErrorCode? ErrorCode { get; init; }
    public List<ValidationFailure> Errors { get; init; } = new();

    public static Result Ok(string? message = null) =>
        new()
        {
            Success = true,
            Message = message ?? "Operation successful",
            StatusCode = 200
        };

    public static Result Fail(string message, ResultErrorCode errorCode = ResultErrorCode.ValidationError) =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 400,
            ErrorCode = errorCode
        };

    public static Result NotFound(string message = "Resource not found") =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 404,
            ErrorCode = ResultErrorCode.NotFound
        };

    public static Result Unauthorized(string message = "Unauthorized") =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 401,
            ErrorCode = ResultErrorCode.Unauthorized
        };

    public static Result Forbidden(string message = "Forbidden") =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 403,
            ErrorCode = ResultErrorCode.Forbidden
        };

    public static Result Conflict(string message) =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 409,
            ErrorCode = ResultErrorCode.Conflict
        };

    public static Result DatabaseError(string message = "Database operation failed") =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 500,
            ErrorCode = ResultErrorCode.DatabaseError
        };

    public static Result ValidationError(List<ValidationFailure> failures) =>
        new()
        {
            Success = false,
            Message = "Validation failed",
            StatusCode = 400,
            ErrorCode = ResultErrorCode.ValidationError,
            Errors = failures
        };

    public static Result InsufficientBalance(string message = "Insufficient balance") =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 400,
            ErrorCode = ResultErrorCode.InsufficientBalance
        };
}

public class Result<T> : Result
{
    public T? Data { get; init; }

    public static Result<T> Ok(T data, string? message = null) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation successful",
            StatusCode = 200
        };

    public static new Result<T> Fail(string message, ResultErrorCode errorCode = ResultErrorCode.ValidationError) =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 400,
            ErrorCode = errorCode
        };

    public static new Result<T> NotFound(string message = "Resource not found") =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 404,
            ErrorCode = ResultErrorCode.NotFound
        };

    public static new Result<T> Unauthorized(string message = "Unauthorized") =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 401,
            ErrorCode = ResultErrorCode.Unauthorized
        };

    public static new Result<T> Forbidden(string message = "Forbidden") =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 403,
            ErrorCode = ResultErrorCode.Forbidden
        };

    public static new Result<T> Conflict(string message) =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 409,
            ErrorCode = ResultErrorCode.Conflict
        };

    public static new Result<T> DatabaseError(string message = "Database operation failed") =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 500,
            ErrorCode = ResultErrorCode.DatabaseError
        };
}

public class PagedResult<T> : Result<IEnumerable<T>>
{
    public int PageIndex { get; init; }
    public int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasPreviousPage => PageIndex > 1;
    public bool HasNextPage => PageIndex < TotalPages;

    public static PagedResult<T> Ok(
        IEnumerable<T> data,
        int pageIndex,
        int pageSize,
        int totalCount,
        string? message = null) =>
        new()
        {
            Success = true,
            Data = data,
            Message = message ?? "Operation successful",
            StatusCode = 200,
            PageIndex = pageIndex,
            PageSize = pageSize,
            TotalCount = totalCount
        };

    public static new PagedResult<T> Fail(
        string message,
        ResultErrorCode errorCode = ResultErrorCode.ValidationError) =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 400,
            ErrorCode = errorCode,
            Data = Enumerable.Empty<T>()
        };

    public static new PagedResult<T> NotFound(string message = "Resource not found") =>
        new()
        {
            Success = false,
            Message = message,
            StatusCode = 404,
            ErrorCode = ResultErrorCode.NotFound,
            Data = Enumerable.Empty<T>()
        };
}
