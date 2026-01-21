using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Roles.DTOs;

namespace Application.UseCases.Roles.CQRS.Queries.GetById;

public sealed record RoleGetByIdQuery(Guid Id) : IRequest<OperationResult<RoleDTO>>;
