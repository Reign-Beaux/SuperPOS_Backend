using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Products.CQRS.Commands.Delete;

public sealed record ProductDeleteCommand(Guid Id) : IRequest<OperationResult<VoidResult>>;
