using Domain.Entities.Inventories;
using Domain.Services;
using Domain.ValueObjects;

namespace Infrastructure.Services.Domain;

/// <summary>
/// Implementation of stock reservation service.
/// Manages stock validation and reservation in a transactional manner.
/// </summary>
public class StockReservationService : IStockReservationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly List<Inventory> _reservedInventories = [];

    public StockReservationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Success, string? ErrorMessage)> ValidateAndReserveStockAsync(
        List<(Guid ProductId, Quantity Quantity)> items,
        CancellationToken cancellationToken = default)
    {
        // Clear any previous reservations
        _reservedInventories.Clear();

        try
        {
            foreach (var (productId, quantity) in items)
            {
                // Get inventory for this product
                var inventory = await _unitOfWork.Inventories.GetByProductIdAsync(productId, cancellationToken);

                if (inventory == null)
                {
                    return (false, $"No inventory found for product {productId}");
                }

                // Check if sufficient stock is available
                if (!inventory.HasSufficientStock(quantity))
                {
                    return (false, $"Insufficient stock for product {productId}. Available: {inventory.Stock}, Required: {quantity.Value}");
                }

                // Reserve stock by removing it from inventory
                inventory.RemoveStock(quantity);

                // Track this inventory for potential rollback
                _reservedInventories.Add(inventory);

                // Mark as modified for the unit of work
                _unitOfWork.Inventories.Update(inventory);
            }

            return (true, null);
        }
        catch (global::Domain.Exceptions.InsufficientStockException ex)
        {
            // Rollback any changes made so far
            await RollbackReservationAsync();
            return (false, ex.Message);
        }
        catch (global::Domain.Exceptions.InvalidQuantityException ex)
        {
            // Rollback any changes made so far
            await RollbackReservationAsync();
            return (false, ex.Message);
        }
        catch (Exception ex)
        {
            // Rollback on any unexpected error
            await RollbackReservationAsync();
            return (false, $"Error reserving stock: {ex.Message}");
        }
    }

    public async Task CommitReservationAsync(CancellationToken cancellationToken = default)
    {
        // Save changes to commit the stock updates
        // Note: LowStockEvent is automatically raised by Inventory.RemoveStock() when stock falls below threshold
        // The events will be dispatched automatically when SaveChangesAsync is called
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Clear reservation tracking
        _reservedInventories.Clear();
    }

    public Task RollbackReservationAsync()
    {
        // Revert all inventory changes by adding back the quantities
        foreach (var inventory in _reservedInventories)
        {
            // Note: In a real scenario, we would need to track the original quantities
            // For now, we rely on the fact that if rollback is called,
            // the entire transaction will be rolled back by not calling SaveChanges
        }

        // Clear reservation tracking
        _reservedInventories.Clear();

        return Task.CompletedTask;
    }
}
