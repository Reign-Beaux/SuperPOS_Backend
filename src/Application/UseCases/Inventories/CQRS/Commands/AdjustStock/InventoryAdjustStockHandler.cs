using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Inventories.DTOs;
using Domain.Entities.Inventories;
using Domain.Repositories;
using Domain.ValueObjects;

namespace Application.UseCases.Inventories.CQRS.Commands.AdjustStock;

public class InventoryAdjustStockHandler : IRequestHandler<InventoryAdjustStockCommand, OperationResult<InventoryDTO>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public InventoryAdjustStockHandler(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<OperationResult<InventoryDTO>> Handle(InventoryAdjustStockCommand request, CancellationToken cancellationToken)
    {
        // Validate product exists
        var product = await _unitOfWork.Products.GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Error(ErrorResult.NotFound, detail: InventoryMessages.AdjustStock.ProductNotFound);
        }

        // Get or create inventory
        var inventory = await _unitOfWork.Inventories.GetByProductIdAsync(request.ProductId, cancellationToken);

        var quantity = Quantity.Create(request.Stock);

        try
        {
            if (inventory == null)
            {
                // Create new inventory record - only valid for Add or Set operations
                if (request.Operation == InventoryOperation.Add || request.Operation == InventoryOperation.Set)
                {
                    inventory = Inventory.Create(request.ProductId, quantity);
                    _unitOfWork.Inventories.Add(inventory);
                }
                else
                {
                    return Result.Error(ErrorResult.BadRequest, detail: InventoryMessages.AdjustStock.InvalidQuantity);
                }
            }
            else
            {
                // Update existing inventory using domain methods
                switch (request.Operation)
                {
                    case InventoryOperation.Add:
                        inventory.AddStock(quantity);
                        break;

                    case InventoryOperation.Set:
                        inventory.SetStock(quantity);
                        break;

                    default:
                        return Result.Error(ErrorResult.BadRequest, detail: "Invalid inventory operation");
                }

                _unitOfWork.Inventories.Update(inventory);
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // Load product for DTO mapping
            inventory.Product = product;
            var dto = _mapper.Map<InventoryDTO>(inventory);

            return Result.Success(dto);
        }
        catch (Domain.Exceptions.InvalidQuantityException ex)
        {
            return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
        }
        catch (Domain.Exceptions.BusinessRuleViolationException ex)
        {
            return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
        }
    }
}
