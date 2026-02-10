using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Roles.DTOs;

namespace Application.UseCases.Roles.CQRS.Commands.Create;

public record CreateRoleCommand(
    string Name,
    string? Description
) : IRequest<OperationResult<Guid>>;
