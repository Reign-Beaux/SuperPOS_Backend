using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Roles.CQRS.Commands.Update;

public sealed record RoleUpdateCommand(
    Guid Id,
    string Name,
    string? Description) : IRequest<OperationResult<VoidResult>>;
