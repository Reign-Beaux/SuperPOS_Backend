using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Users.DTOs;

namespace Application.UseCases.Users.CQRS.Commands.Create;

public record CreateUserCommand(
    string Name,
    string FirstLastname,
    string? SecondLastname,
    string Email,
    string Password,
    string? Phone,
    Guid RoleId
) : IRequest<OperationResult<UserDTO>>;
