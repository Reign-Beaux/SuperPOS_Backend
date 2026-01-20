using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Articles.CQRS.Commands.Update;

public sealed record ArticleUpdateCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Barcode) : IRequest<OperationResult<VoidResult>>;
