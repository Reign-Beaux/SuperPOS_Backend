using Domain.Repositories;
using Domain.Services;

namespace Infrastructure.Services;

/// <summary>
/// Implementation of product uniqueness checker using the repository.
/// </summary>
public class ProductUniquenessChecker : IProductUniquenessChecker
{
    private readonly IProductRepository _productRepository;

    public ProductUniquenessChecker(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public async Task<bool> IsNameUniqueAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        // Check if a product with this name already exists
        var exists = await _productRepository.ExistsByNameAsync(name, excludeId, cancellationToken);

        // Return true if it does NOT exist (is unique)
        return !exists;
    }

    public async Task<bool> IsBarcodeUniqueAsync(string barcode, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return true; // Empty barcode is considered "unique" (allowed)

        // Check if a product with this barcode already exists
        var exists = await _productRepository.ExistsByBarcodeAsync(barcode, excludeId, cancellationToken);

        // Return true if it does NOT exist (is unique)
        return !exists;
    }
}
