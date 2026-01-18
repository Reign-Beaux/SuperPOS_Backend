namespace Application.DesignPatterns.OperationResults;

public interface IOperationResult
{
    StatusResult Status { get; }
    bool IsSuccess { get; }
    Error? Error { get; }
}
