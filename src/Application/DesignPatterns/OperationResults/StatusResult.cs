namespace DesignPatterns.OperationResults;

public enum StatusResult
{
    Ok,
    NoContent,
    Created,
    Exists,
    Conflict,
    BadRequest,
    NotFound,
    InternalServerError,
    Forbidden,
    ServiceUnavailable,
    GatewayTimeout
}

public enum ErrorResult
{
    Exists,
    Conflict,
    BadRequest,
    NotFound,
    InternalServerError,
    Forbidden,
    ServiceUnavailable,
    GatewayTimeout
}

public static class ErrorResultExtensions
{
    public static StatusResult ToStatusResult(this ErrorResult error)
        => Enum.Parse<StatusResult>(error.ToString());

    public static ErrorResult ToErrorResult(this StatusResult status)
        => Enum.Parse<ErrorResult>(status.ToString());
}
