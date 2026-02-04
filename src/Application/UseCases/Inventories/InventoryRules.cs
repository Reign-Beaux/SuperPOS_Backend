using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Inventories;
using Domain.Entities.Products;

namespace Application.UseCases.Inventories;

public class InventoryRules
{
    private readonly IUnitOfWork _unitOfWork;

    public InventoryRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> ProductExistsAsync(Guid productId)
    {
        var product = await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
        return product != null;
    }

    public bool IsValidQuantityAfterOperation(int currentQuantity, int adjustmentQuantity, InventoryOperation operation)
    {
        var resultQuantity = operation switch
        {
            InventoryOperation.Add => currentQuantity + adjustmentQuantity,
            InventoryOperation.Set => adjustmentQuantity,
            _ => currentQuantity
        };

        return resultQuantity >= 0;
    }

    public async Task<bool> HasSufficientStock(Guid productId, int requiredQuantity)
    {
        var inventory = await _unitOfWork.Repository<Inventory>()
            .FirstOrDefaultAsync(i => i.ProductId == productId);

        return inventory != null && inventory.Quantity >= requiredQuantity;
    }
}

public enum InventoryOperation
{
    Add,
    Set
}
