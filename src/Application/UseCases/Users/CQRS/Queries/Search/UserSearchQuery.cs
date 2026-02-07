using Application.DesignPatterns.OperationResults;
using Application.UseCases.Users.DTOs;
using Application.DesignPatterns.Mediators.Interfaces;

namespace Application.UseCases.Users.CQRS.Queries.Search;

public sealed record UserSearchQuery(string Term) : IRequest<OperationResult<IEnumerable<UserDTO>>>;
