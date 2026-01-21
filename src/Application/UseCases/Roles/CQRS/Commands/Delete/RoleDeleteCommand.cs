using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Roles.CQRS.Commands.Delete;

public sealed record RoleDeleteCommand(Guid Id) : IRequest<OperationResult<VoidResult>>;
