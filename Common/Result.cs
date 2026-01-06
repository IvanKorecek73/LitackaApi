namespace LitackaApi.Common;

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }
    public int StatusCode { get; }

    private Result(bool isSuccess, T? value, string? errorMessage, int statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        ErrorMessage = errorMessage;
        StatusCode = statusCode;
    }

    // ===== SUCCESS =====

    public static Result<T> Success(T value, int statusCode = 200)
        => new Result<T>(true, value, null, statusCode);

    // ===== FAILURES =====

    public static Result<T> NotFound(string errorMessage)
        => new Result<T>(false, default, errorMessage, 404);

    public static Result<T> Unauthorized(string errorMessage)
        => new Result<T>(false, default, errorMessage, 401);

    public static Result<T> Forbidden(string errorMessage)
        => new Result<T>(false, default, errorMessage, 403);

    public static Result<T> BadRequest(string errorMessage)
        => new Result<T>(false, default, errorMessage, 400);

    public static Result<T> Error(string errorMessage, int statusCode = 500)
        => new Result<T>(false, default, errorMessage, statusCode);
}
