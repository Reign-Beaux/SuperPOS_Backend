using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Sales.CQRS.Commands.Cancel;

/// <summary>
/// Command to cancel a sale and restore inventory.
/// </summary>
public record CancelSaleCommand(
    Guid SaleId,
    Guid UserId,
    string Reason
) : IRequest<OperationResult<VoidResult>>;
