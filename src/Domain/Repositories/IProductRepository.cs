using Domain.Entities.Products;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Product aggregate root.
/// Provides specific operations for product management.
/// </summary>
public interface IProductRepository : IRepositoryBase<Product>
{
    /// <summary>
    /// Checks if a product with the given name already exists (case-insensitive).
    /// </summary>
    /// <param name="name">Product name to check</param>
    /// <param name="excludeId">Optional product ID to exclude from the check (for updates)</param>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a product with the given barcode already exists.
    /// </summary>
    /// <param name="barcode">Barcode to check</param>
    /// <param name="excludeId">Optional product ID to exclude from the check (for updates)</param>
    Task<bool> ExistsByBarcodeAsync(string barcode, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a product by its barcode.
    /// </summary>
    Task<Product?> GetByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by name (partial match).
    /// </summary>
    Task<IReadOnlyList<Product>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default);

    /// <summary>
    /// Searches products by name or barcode (partial match, case-insensitive).
    /// Returns up to the specified maximum number of results.
    /// </summary>
    /// <param name="searchTerm">Search term to match against name or barcode</param>
    /// <param name="maxResults">Maximum number of results to return (default: 20)</param>
    Task<IReadOnlyList<Product>> SearchAsync(string searchTerm, int maxResults = 20, CancellationToken cancellationToken = default);
}
