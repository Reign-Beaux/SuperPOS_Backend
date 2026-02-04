using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Inventories.DTOs;

namespace Application.UseCases.Inventories.CQRS.Queries.GetAll;

public record InventoryGetAllQuery : IRequest<OperationResult<List<InventoryDTO>>>;
