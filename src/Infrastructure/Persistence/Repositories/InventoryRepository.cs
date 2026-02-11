using Domain.Entities.Inventories;
using Domain.Entities.Products;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Inventory aggregate root.
/// </summary>
public sealed class InventoryRepository : RepositoryBase<Inventory>, IInventoryRepository
{
    public InventoryRepository(SuperPOSDbContext context) : base(context)
    {
    }

    public async Task<Inventory?> GetByProductIdAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.ProductId == productId && i.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Inventory>> GetLowStockItemsAsync(int threshold = 10, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.Stock <= threshold && i.Stock > 0 && i.DeletedAt == null)
            .OrderBy(i => i.Stock)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Inventory>> GetOutOfStockItemsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(i => i.Stock == 0 && i.DeletedAt == null)
            .ToListAsync(cancellationToken);
    }

    public async Task<Inventory?> GetByIdWithProductAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var inventory = await GetByIdAsync(id, cancellationToken);

        if (inventory != null)
        {
            var product = await _context.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == inventory.ProductId && p.DeletedAt == null, cancellationToken);
            if (product != null)
                inventory.Product = product;
        }

        return inventory;
    }
}
