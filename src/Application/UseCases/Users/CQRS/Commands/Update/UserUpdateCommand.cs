using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Users.CQRS.Commands.Update;

public sealed record UserUpdateCommand(
    Guid Id,
    string Name,
    string FirstLastname,
    string? SecondLastname,
    string Email,
    string? Password,
    string? Phone,
    Guid RoleId) : IRequest<OperationResult<VoidResult>>;
