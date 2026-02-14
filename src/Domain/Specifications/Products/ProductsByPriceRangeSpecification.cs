using Domain.Entities.Products;

namespace Domain.Specifications.Products;

/// <summary>
/// Specification to get products within a price range.
/// Demonstrates filtering with multiple conditions and custom ordering.
/// </summary>
public class ProductsByPriceRangeSpecification : BaseSpecification<Product>
{
    /// <summary>
    /// Gets products with UnitPrice between min and max (inclusive).
    /// </summary>
    public ProductsByPriceRangeSpecification(decimal minPrice, decimal maxPrice)
        : base(p => p.UnitPrice >= minPrice && p.UnitPrice <= maxPrice)
    {
        // Order by price descending (highest first), then by name
        AddOrderByDescending(p => p.UnitPrice);
        AddThenBy(p => p.Name);

        SetTracking(false);
        SetSplitQuery(false);
    }

    /// <summary>
    /// Gets products within price range with pagination.
    /// </summary>
    public ProductsByPriceRangeSpecification(decimal minPrice, decimal maxPrice, int pageIndex, int pageSize)
        : base(p => p.UnitPrice >= minPrice && p.UnitPrice <= maxPrice)
    {
        AddOrderByDescending(p => p.UnitPrice);
        AddThenBy(p => p.Name);
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);

        SetTracking(false);
        SetSplitQuery(false);
    }
}
