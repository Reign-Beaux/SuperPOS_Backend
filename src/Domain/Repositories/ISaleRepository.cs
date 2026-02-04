using Domain.Entities.Sales;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Sale aggregate root.
/// Provides specific operations for sales management.
/// </summary>
public interface ISaleRepository : IRepositoryBase<Sale>
{
    /// <summary>
    /// Gets all sales for a specific customer.
    /// </summary>
    Task<IReadOnlyList<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sales made by a specific user.
    /// </summary>
    Task<IReadOnlyList<Sale>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets sales within a date range.
    /// </summary>
    Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a sale by ID with all related details (SaleDetails, Customer, User, Products).
    /// </summary>
    Task<Sale?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all sales with their details loaded.
    /// </summary>
    Task<IReadOnlyList<Sale>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default);
}
