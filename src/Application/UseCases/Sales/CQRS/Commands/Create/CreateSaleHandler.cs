using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Domain.Entities.Sales;
using Domain.Repositories;
using Domain.Services;
using Domain.ValueObjects;

namespace Application.UseCases.Sales.CQRS.Commands.Create;

public class CreateSaleHandler : IRequestHandler<CreateSaleCommand, OperationResult<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISaleValidationService _saleValidationService;
    private readonly IStockReservationService _stockReservationService;

    public CreateSaleHandler(
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ISaleValidationService saleValidationService,
        IStockReservationService stockReservationService)
    {
        _unitOfWork = unitOfWork;
        
        _saleValidationService = saleValidationService;
        _stockReservationService = stockReservationService;
    }

    public async Task<OperationResult<Guid>> Handle(CreateSaleCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate basic input
        if (request.Items == null || request.Items.Count == 0)
        {
            return Result.Error(ErrorResult.BadRequest, detail: SaleMessages.Create.EmptyItems);
        }

        // 2. Validate that customer exists
        if (!await _saleValidationService.CustomerExistsAsync(request.CustomerId, cancellationToken))
        {
            return Result.Error(ErrorResult.NotFound, detail: SaleMessages.Create.CustomerNotFound);
        }

        // 3. Validate that user exists
        if (!await _saleValidationService.UserExistsAsync(request.UserId, cancellationToken))
        {
            return Result.Error(ErrorResult.NotFound, detail: SaleMessages.Create.UserNotFound);
        }

        // 4. Prepare items with product data
        var itemsWithPrices = new List<(Guid ProductId, Quantity Quantity, decimal UnitPrice)>();

        foreach (var item in request.Items)
        {
            // Validate product exists
            var product = await _unitOfWork.Products.GetByIdAsync(item.ProductId, cancellationToken);
            if (product == null)
            {
                return Result.Error(
                    ErrorResult.NotFound,
                    detail: $"Product with ID {item.ProductId} not found");
            }

            var quantity = Quantity.Create(item.Quantity);
            var unitPrice = product.UnitPrice;

            itemsWithPrices.Add((item.ProductId, quantity, unitPrice));
        }

        // 5. Validate and reserve stock
        var stockItems = itemsWithPrices.Select(x => (x.ProductId, x.Quantity)).ToList();
        var (success, errorMessage) = await _stockReservationService.ValidateAndReserveStockAsync(
            stockItems,
            cancellationToken);

        if (!success)
        {
            return Result.Error(ErrorResult.BadRequest, detail: errorMessage);
        }

        try
        {
            // 6. Create sale using domain factory method
            var sale = Sale.Create(request.CustomerId, request.UserId, itemsWithPrices);

            // 7. Save sale (this generates the Id)
            _unitOfWork.Sales.Add(sale);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 8. Finalize details (sets SaleId on all details)
            sale.FinalizeDetails();

            // 9. Commit stock reservation
            await _stockReservationService.CommitReservationAsync(cancellationToken);

            // 10. Reload sale with all relationships for DTO mapping
            var createdSale = await _unitOfWork.Sales.GetByIdWithDetailsAsync(sale.Id, cancellationToken);

            if (createdSale == null)
            {
                return Result.Error(ErrorResult.BadRequest, detail: "Error loading created sale");
            }

            return Result.Created(createdSale.Id);
        }
        catch (Domain.Exceptions.DomainException ex)
        {
            // Domain exception - rollback stock and return error
            await _stockReservationService.RollbackReservationAsync();
            return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
        }
        catch (Exception)
        {
            // Unexpected error - rollback stock and rethrow
            await _stockReservationService.RollbackReservationAsync();
            throw;
        }
    }
}
