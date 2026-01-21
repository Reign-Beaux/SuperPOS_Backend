using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Users.DTOs;

namespace Application.UseCases.Users.CQRS.Queries.GetById;

public sealed record UserGetByIdQuery(Guid Id) : IRequest<OperationResult<UserDTO>>;
