using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Products.DTOs;

namespace Application.UseCases.Products.CQRS.Commands.Create;

public record CreateProductCommand(
    string Name,
    string? Description,
    string? Barcode,
    decimal UnitPrice
) : IRequest<OperationResult<ProductDTO>>;
