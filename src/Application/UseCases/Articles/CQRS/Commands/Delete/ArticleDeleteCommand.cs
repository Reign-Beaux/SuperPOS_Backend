using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Articles.CQRS.Commands.Delete;

public sealed record ArticleDeleteCommand(Guid Id) : IRequest<OperationResult<VoidResult>>;
