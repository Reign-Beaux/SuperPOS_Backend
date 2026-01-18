namespace Application.DesignPatterns.OperationResults;

public static class Result
{
    public static OperationResult<VoidResult> Success()
        => new(StatusResult.Ok, new VoidResult());

    public static OperationResult<T> Success<T>(T value)
        => new(StatusResult.Ok, value);

    public static OperationResult<VoidResult> NoContent()
        => new(StatusResult.NoContent, new VoidResult());

    public static OperationResult<Guid> Created(Guid id)
        => new(StatusResult.Created, id);

    public static OperationResult<VoidResult> Error(ErrorResult errorResult, string detail)
        => new(
            errorResult.ToStatusResult(),
            new VoidResult(),
            new Error { Title = errorResult.ToString(), Detail = detail });

}
