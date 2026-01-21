using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Users.CQRS.Commands.Delete;

public sealed record UserDeleteCommand(Guid Id) : IRequest<OperationResult<VoidResult>>;
