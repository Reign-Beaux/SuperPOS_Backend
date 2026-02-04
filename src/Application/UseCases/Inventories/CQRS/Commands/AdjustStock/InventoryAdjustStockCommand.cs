using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Inventories.DTOs;

namespace Application.UseCases.Inventories.CQRS.Commands.AdjustStock;

public record InventoryAdjustStockCommand(
    Guid ProductId,
    int Quantity,
    InventoryOperation Operation
) : IRequest<OperationResult<InventoryDTO>>;
