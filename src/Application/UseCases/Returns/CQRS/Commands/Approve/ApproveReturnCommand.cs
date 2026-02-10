using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Returns.CQRS.Commands.Approve;

public record ApproveReturnCommand(
    Guid ReturnId,
    Guid ApprovedByUserId
) : IRequest<OperationResult<VoidResult>>;
