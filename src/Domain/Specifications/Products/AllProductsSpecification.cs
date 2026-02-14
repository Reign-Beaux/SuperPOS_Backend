using Domain.Entities.Products;

namespace Domain.Specifications.Products;

/// <summary>
/// Specification to get all active products with default ordering.
/// Simple example of using specifications for basic queries.
/// </summary>
public class AllProductsSpecification : BaseSpecification<Product>
{
    /// <summary>
    /// Gets all products ordered by name.
    /// </summary>
    public AllProductsSpecification()
    {
        // No filter criteria - get all products (soft delete filter applied by repository)
        AddOrderBy(p => p.Name);

        SetTracking(false);
        SetSplitQuery(false);
    }

    /// <summary>
    /// Gets all products with pagination.
    /// </summary>
    public AllProductsSpecification(int pageIndex, int pageSize)
    {
        AddOrderBy(p => p.Name);
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);

        SetTracking(false);
        SetSplitQuery(false);
    }
}
