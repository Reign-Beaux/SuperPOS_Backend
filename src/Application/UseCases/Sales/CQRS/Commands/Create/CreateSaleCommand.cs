using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Sales.DTOs;

namespace Application.UseCases.Sales.CQRS.Commands.Create;

public record SaleItemInput(Guid ProductId, int Quantity);

public record CreateSaleCommand(
    Guid CustomerId,
    Guid UserId,
    List<SaleItemInput> Items
) : IRequest<OperationResult<Guid>>;
