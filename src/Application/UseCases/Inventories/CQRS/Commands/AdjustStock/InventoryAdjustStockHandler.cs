using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence.UnitOfWorks;
using Application.UseCases.Inventories.DTOs;
using Domain.Entities.Inventories;
using Domain.Entities.Products;

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
        var rules = new InventoryRules(_unitOfWork);

        // Validar que el producto existe
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(request.ProductId, cancellationToken);
        if (product == null)
        {
            return Result.Error(ErrorResult.NotFound, detail: InventoryMessages.AdjustStock.ProductNotFound);
        }

        // Obtener o crear inventario
        var inventoryRepository = _unitOfWork.Repository<Inventory>();
        var inventory = await inventoryRepository.FirstOrDefaultAsync(i => i.ProductId == request.ProductId, cancellationToken);

        var currentQuantity = inventory?.Quantity ?? 0;

        // Validar que la cantidad resultante no sea negativa
        if (!rules.IsValidQuantityAfterOperation(currentQuantity, request.Quantity, request.Operation))
        {
            return Result.Error(ErrorResult.BadRequest, detail: InventoryMessages.AdjustStock.InvalidQuantity);
        }

        // Aplicar la operación
        if (inventory == null)
        {
            // Crear nuevo registro de inventario
            inventory = new Inventory
            {
                ProductId = request.ProductId,
                Quantity = request.Operation == InventoryOperation.Add ? request.Quantity : request.Quantity
            };
            inventoryRepository.Add(inventory);
        }
        else
        {
            // Actualizar inventario existente
            inventory.Quantity = request.Operation switch
            {
                InventoryOperation.Add => inventory.Quantity + request.Quantity,
                InventoryOperation.Set => request.Quantity,
                _ => inventory.Quantity
            };
            inventoryRepository.Update(inventory);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Mapear a DTO con el producto cargado
        inventory.Product = product;
        var dto = _mapper.Map<InventoryDTO>(inventory);

        return Result.Success(dto);
    }
}
