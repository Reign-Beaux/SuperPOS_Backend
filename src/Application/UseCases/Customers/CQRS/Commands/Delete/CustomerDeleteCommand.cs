using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Customers.CQRS.Commands.Delete;

public sealed record CustomerDeleteCommand(Guid Id) : IRequest<OperationResult<VoidResult>>;
