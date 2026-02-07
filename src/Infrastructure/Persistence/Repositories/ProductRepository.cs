using Domain.Entities.Products;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Product aggregate root.
/// </summary>
public sealed class ProductRepository : RepositoryBase<Product>, IProductRepository
{
    public ProductRepository(SuperPOSDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(p => p.Name == name && p.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<bool> ExistsByBarcodeAsync(string barcode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return false;

        var query = _dbSet.Where(p => p.Barcode == barcode && p.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(p => p.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        return await _dbSet
            .Where(p => p.Barcode == barcode && p.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync(cancellationToken);

        var normalizedTerm = searchTerm.ToLower();

        return await _dbSet
            .Where(p => p.DeletedAt == null && p.Name.ToLower().Contains(normalizedTerm))
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, int maxResults = 20, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return Array.Empty<Product>();

        var normalizedTerm = searchTerm.ToLower();

        return await _dbSet
            .Where(p => p.DeletedAt == null &&
                       (p.Name.ToLower().Contains(normalizedTerm) ||
                        (p.Barcode != null && p.Barcode.ToLower().Contains(normalizedTerm))))
            .OrderBy(p => p.Name)
            .Take(maxResults)
            .ToListAsync(cancellationToken);
    }
}
