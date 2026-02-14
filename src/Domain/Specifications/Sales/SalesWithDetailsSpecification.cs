using Domain.Entities.Sales;

namespace Domain.Specifications.Sales;

/// <summary>
/// Specification to get sales with all related entities loaded (Customer, User, SaleDetails with Products).
/// Demonstrates eager loading using Include expressions.
/// </summary>
public class SalesWithDetailsSpecification : BaseSpecification<Sale>
{
    /// <summary>
    /// Gets all sales with complete details (Customer, User, SaleDetails, Products).
    /// </summary>
    public SalesWithDetailsSpecification()
    {
        // Eager load related entities
        AddInclude(s => s.Customer);
        AddInclude(s => s.User);
        AddInclude(s => s.SaleDetails);

        // Deep navigation: load Product for each SaleDetail
        AddInclude("SaleDetails.Product");

        // Default ordering: most recent sales first
        AddOrderByDescending(s => s.CreatedAt);

        // Read-only query optimization
        SetTracking(false);

        // Use split query to avoid cartesian explosion with multiple collections
        SetSplitQuery(true);
    }

    /// <summary>
    /// Gets sales within a date range with complete details.
    /// </summary>
    public SalesWithDetailsSpecification(DateTime startDate, DateTime endDate)
        : base(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate)
    {
        AddInclude(s => s.Customer);
        AddInclude(s => s.User);
        AddInclude(s => s.SaleDetails);
        AddInclude("SaleDetails.Product");

        AddOrderByDescending(s => s.CreatedAt);

        SetTracking(false);
        SetSplitQuery(true);
    }

    /// <summary>
    /// Gets sales for a specific customer with pagination.
    /// </summary>
    public SalesWithDetailsSpecification(Guid customerId, int pageIndex, int pageSize)
        : base(s => s.CustomerId == customerId)
    {
        AddInclude(s => s.Customer);
        AddInclude(s => s.User);
        AddInclude(s => s.SaleDetails);
        AddInclude("SaleDetails.Product");

        AddOrderByDescending(s => s.CreatedAt);
        ApplyPaging((pageIndex - 1) * pageSize, pageSize);

        SetTracking(false);
        SetSplitQuery(true);
    }

    /// <summary>
    /// Gets sales with total above minimum amount.
    /// </summary>
    public SalesWithDetailsSpecification(decimal minTotalAmount)
        : base(s => s.TotalAmount >= minTotalAmount)
    {
        AddInclude(s => s.Customer);
        AddInclude(s => s.User);
        AddInclude(s => s.SaleDetails);
        AddInclude("SaleDetails.Product");

        AddOrderByDescending(s => s.TotalAmount);
        AddThenByDescending(s => s.CreatedAt);

        SetTracking(false);
        SetSplitQuery(true);
    }
}
