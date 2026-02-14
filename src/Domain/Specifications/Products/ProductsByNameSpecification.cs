using Domain.Entities.Products;

namespace Domain.Specifications.Products;

/// <summary>
/// Specification to search products by name (case-insensitive partial match).
/// Example usage for filtered queries with ordering and pagination.
/// </summary>
public class ProductsByNameSpecification : BaseSpecification<Product>
{
    /// <summary>
    /// Searches products by name containing the search term.
    /// </summary>
    public ProductsByNameSpecification(string searchTerm)
        : base(p => p.Name.Contains(searchTerm))
    {
        // Default ordering: by name ascending
        AddOrderBy(p => p.Name);

        // Read-only query optimization
        SetTracking(false);
        SetSplitQuery(false); // Single table, no includes
    }

    /// <summary>
    /// Searches products by name with pagination.
    /// </summary>
    public ProductsByNameSpecification(string searchTerm, int pageIndex, int pageSize)
        : base(p => p.Name.Contains(searchTerm))
    {
        AddOrderBy(p => p.Name);
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);

        SetTracking(false);
        SetSplitQuery(false);
    }
}
