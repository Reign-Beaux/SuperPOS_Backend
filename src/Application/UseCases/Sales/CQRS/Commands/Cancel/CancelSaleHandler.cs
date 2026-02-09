using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Domain.Entities.Sales;

namespace Application.UseCases.Sales.CQRS.Commands.Cancel;

/// <summary>
/// Handler for cancelling sales.
/// Cancels the sale and triggers domain event to restore inventory.
/// </summary>
public class CancelSaleHandler : IRequestHandler<CancelSaleCommand, OperationResult<VoidResult>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CancelSaleHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<OperationResult<VoidResult>> Handle(CancelSaleCommand request, CancellationToken cancellationToken)
    {
        // 1. Get sale with details
        var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(request.SaleId, cancellationToken);

        if (sale == null)
            return Result.Error(ErrorResult.NotFound, detail: SaleMessages.NotFound.WithId(request.SaleId));

        // 2. Cancel the sale (this raises SaleCancelledEvent)
        try
        {
            sale.Cancel(request.UserId, request.Reason);
        }
        catch (global::Domain.Exceptions.BusinessRuleViolationException ex)
        {
            return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
        }

        // 3. Save changes (will dispatch the SaleCancelledEvent)
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
