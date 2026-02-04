using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Inventories.DTOs;

namespace Application.UseCases.Inventories.CQRS.Queries.GetByProductId;

public record InventoryGetByProductIdQuery(Guid ProductId) : IRequest<OperationResult<InventoryDTO>>;
