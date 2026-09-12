namespace TaskFlow.Application.Common;

public enum ResultStatus
{
    Success,
    NotFound,
    ValidationFailed
}

public sealed class Result
{
    public ResultStatus Status { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(ResultStatus status, IReadOnlyList<string> errors)
    {
        Status = status;
        Errors = errors;
    }

    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result Success() => new(ResultStatus.Success, []);
    public static Result NotFound() => new(ResultStatus.NotFound, []);
    public static Result Invalid(IReadOnlyList<string> errors) => new(ResultStatus.ValidationFailed, errors);
}

public sealed class Result<T>
{
    public ResultStatus Status { get; }
    public T? Value { get; }
    public IReadOnlyList<string> Errors { get; }

    private Result(ResultStatus status, T? value, IReadOnlyList<string> errors)
    {
        Status = status;
        Value = value;
        Errors = errors;
    }

    public bool IsSuccess => Status == ResultStatus.Success;

    public static Result<T> Success(T value) => new(ResultStatus.Success, value, []);
    public static Result<T> NotFound() => new(ResultStatus.NotFound, default, []);
    public static Result<T> Invalid(IReadOnlyList<string> errors) => new(ResultStatus.ValidationFailed, default, errors);
}
