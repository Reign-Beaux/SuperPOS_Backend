using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Domain.Entities.Returns;
using MapsterMapper;

namespace Application.UseCases.Returns.CQRS.Commands.Create;

public class CreateReturnHandler : IRequestHandler<CreateReturnCommand, OperationResult<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateReturnHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        
    }

    public async Task<OperationResult<Guid>> Handle(CreateReturnCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate sale exists
        var sale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(request.SaleId, cancellationToken);
        if (sale == null)
            return Result.Error(ErrorResult.NotFound, detail: ReturnMessages.Create.SaleNotFound);

        // 2. Validate sale is not cancelled
        if (sale.IsCancelled)
            return Result.Error(ErrorResult.BadRequest, detail: ReturnMessages.Create.SaleCancelled);

        // 3. Validate return window (30 days)
        var daysSinceSale = (DateTime.UtcNow - sale.CreatedAt).TotalDays;
        if (daysSinceSale > 30)
            return Result.Error(ErrorResult.BadRequest, detail: ReturnMessages.Create.ReturnWindowExpired);

        // 4. Validate quantities don't exceed purchased quantities
        foreach (var item in request.Items)
        {
            var saleDetail = sale.SaleDetails.FirstOrDefault(sd => sd.ProductId == item.ProductId);
            if (saleDetail == null || saleDetail.Quantity < item.Quantity)
                return Result.Error(ErrorResult.BadRequest, detail: ReturnMessages.Create.InvalidQuantity);
        }

        // 5. Create return entity
        var returnItems = request.Items
            .Select(i => (i.ProductId, i.Quantity, i.UnitPrice, i.Condition))
            .ToList();

        Return returnEntity;
        try
        {
            returnEntity = Return.Create(
                request.SaleId,
                request.CustomerId,
                request.ProcessedByUserId,
                request.Type,
                request.Reason,
                returnItems
            );
        }
        catch (global::Domain.Exceptions.BusinessRuleViolationException ex)
        {
            return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
        }

        // 6. Finalize details
        returnEntity.FinalizeDetails();

        // 7. Save
        _unitOfWork.Returns.Add(returnEntity);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Load with details and map to DTO
        var createdReturn = await _unitOfWork.Returns.GetByIdWithDetailsAsync(returnEntity.Id, cancellationToken);

        return Result.Created(returnEntity.Id);
    }
}
