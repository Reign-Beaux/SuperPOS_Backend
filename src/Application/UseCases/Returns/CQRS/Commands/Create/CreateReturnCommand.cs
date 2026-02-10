using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Returns.DTOs;
using Domain.Entities.Returns;

namespace Application.UseCases.Returns.CQRS.Commands.Create;

public record CreateReturnCommand(
    Guid SaleId,
    Guid CustomerId,
    Guid ProcessedByUserId,
    ReturnType Type,
    string Reason,
    List<ReturnItemRequest> Items
) : IRequest<OperationResult<Guid>>;

public record ReturnItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    string? Condition
);
