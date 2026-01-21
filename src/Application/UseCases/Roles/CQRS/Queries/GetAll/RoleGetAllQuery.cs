using Application.DesignPatterns.OperationResults;
using Application.UseCases.Roles.DTOs;
using Application.DesignPatterns.Mediators.Interfaces;

namespace Application.UseCases.Roles.CQRS.Queries.GetAll;

public sealed record RoleGetAllQuery : IRequest<OperationResult<IEnumerable<RoleDTO>>>;
