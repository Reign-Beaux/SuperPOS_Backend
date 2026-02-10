using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Returns.CQRS.Commands.Reject;

public record RejectReturnCommand(
    Guid ReturnId,
    Guid RejectedByUserId,
    string RejectionReason
) : IRequest<OperationResult<VoidResult>>;
