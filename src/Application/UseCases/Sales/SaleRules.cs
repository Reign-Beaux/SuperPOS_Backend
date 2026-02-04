using Application.Interfaces.Persistence.UnitOfWorks;
using Domain.Entities.Customers;
using Domain.Entities.Inventories;
using Domain.Entities.Products;
using Domain.Entities.Sales;
using Domain.Entities.Users;

namespace Application.UseCases.Sales;

public class SaleRules
{
    private readonly IUnitOfWork _unitOfWork;

    public SaleRules(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> CustomerExistsAsync(Guid customerId)
    {
        var customer = await _unitOfWork.Repository<Customer>().GetByIdAsync(customerId);
        return customer != null;
    }

    public async Task<bool> UserExistsAsync(Guid userId)
    {
        var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId);
        return user != null;
    }

    public async Task<Product?> GetProductAsync(Guid productId)
    {
        return await _unitOfWork.Repository<Product>().GetByIdAsync(productId);
    }

    public async Task<Inventory?> GetInventoryAsync(Guid productId)
    {
        return await _unitOfWork.Repository<Inventory>()
            .FirstOrDefaultAsync(i => i.ProductId == productId);
    }

    public async Task<bool> HasSufficientStock(Guid productId, int requiredQuantity)
    {
        var inventory = await GetInventoryAsync(productId);
        return inventory != null && inventory.Quantity >= requiredQuantity;
    }

    public async Task<(bool IsValid, string ErrorMessage)> ValidateStockAvailability(List<(Guid ProductId, int Quantity)> items)
    {
        foreach (var (productId, quantity) in items)
        {
            var product = await GetProductAsync(productId);
            if (product == null)
            {
                return (false, SaleMessages.Create.ProductNotFoundWithId(productId));
            }

            var inventory = await GetInventoryAsync(productId);
            if (inventory == null || inventory.Quantity < quantity)
            {
                var available = inventory?.Quantity ?? 0;
                return (false, SaleMessages.Create.WithProductName(product.Name, available, quantity));
            }
        }

        return (true, string.Empty);
    }
}
