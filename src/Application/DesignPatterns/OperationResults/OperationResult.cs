namespace DesignPatterns.OperationResults;

public readonly struct VoidResult { }

public sealed class Error
{
    public required string Title { get; init; }
    public required string Detail { get; init; }
}

public class OperationResult<T> : IOperationResult
{
    public bool IsSuccess => Status is StatusResult.Ok or StatusResult.NoContent or StatusResult.Created;

    public StatusResult Status { get; }

    public Error? Error { get; }

    public T? Value { get; }

    public OperationResult(
        StatusResult status,
        T? value = default,
        Error? error = null)
    {
        Status = status;
        Value = value;
        Error = error;
    }

    public static implicit operator OperationResult<T>(OperationResult<VoidResult> objResult)
    {
        return new OperationResult<T>(objResult.Status, default, objResult.Error);
    }
}
