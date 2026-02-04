using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Products.CQRS.Commands.Update;

public sealed record ProductUpdateCommand(
    Guid Id,
    string Name,
    string? Description,
    string? Barcode,
    decimal UnitPrice) : IRequest<OperationResult<VoidResult>>;
