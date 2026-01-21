using Application.DesignPatterns.OperationResults;
using Application.UseCases.Users.DTOs;
using Application.DesignPatterns.Mediators.Interfaces;

namespace Application.UseCases.Users.CQRS.Queries.GetAll;

public sealed record UserGetAllQuery : IRequest<OperationResult<IEnumerable<UserDTO>>>;
