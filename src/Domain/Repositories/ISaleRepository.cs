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

    // ===============================
    // Dashboard & Analytics Methods
    // ===============================

    /// <summary>
    /// Gets sales grouped by hour of day for analytics.
    /// Returns aggregated data: (Hour, SalesCount, TotalAmount)
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (exclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tuples with hourly aggregations (0-23)</returns>
    Task<IReadOnlyList<(int Hour, int SalesCount, decimal TotalAmount)>> GetHourlySalesAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets top-selling products with quantities and revenue.
    /// Returns aggregated data: (ProductId, ProductName, Quantity, Revenue)
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (exclusive)</param>
    /// <param name="topCount">Number of top products to return (default: 10)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of tuples with product sales metrics</returns>
    Task<IReadOnlyList<(Guid ProductId, string ProductName, int Quantity, decimal Revenue)>> GetTopProductsAsync(
        DateTime startDate,
        DateTime endDate,
        int topCount = 10,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets summary statistics for a date range.
    /// Returns aggregated data: (TotalSales, TotalRevenue, AvgTicketSize, TotalItemsSold)
    /// </summary>
    /// <param name="startDate">Start date (inclusive)</param>
    /// <param name="endDate">End date (exclusive)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple with sales summary metrics</returns>
    Task<(int TotalSales, decimal TotalRevenue, decimal AvgTicketSize, int TotalItemsSold)> GetSalesSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default);
}
