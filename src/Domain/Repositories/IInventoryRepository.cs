using Domain.Entities.Inventories;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Inventory aggregate root.
/// Provides specific operations for inventory management.
/// </summary>
public interface IInventoryRepository : IRepositoryBase<Inventory>
{
    /// <summary>
    /// Gets inventory record for a specific product.
    /// </summary>
    Task<Inventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all inventory items with stock below the specified threshold.
    /// </summary>
    Task<IReadOnlyList<Inventory>> GetLowStockItemsAsync(int threshold = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all inventory items that are out of stock (quantity = 0).
    /// </summary>
    Task<IReadOnlyList<Inventory>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets inventory with product information loaded.
    /// </summary>
    Task<Inventory?> GetByIdWithProductAsync(Guid id, CancellationToken cancellationToken = default);
}
