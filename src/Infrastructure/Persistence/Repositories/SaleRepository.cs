using Domain.Entities.Customers;
using Domain.Entities.Products;
using Domain.Entities.Sales;
using Domain.Entities.Users;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Sale aggregate root.
/// </summary>
public sealed class SaleRepository : RepositoryBase<Sale>, ISaleRepository
{
    public SaleRepository(SuperPOSDbContext context) : base(context)
    {
    }

    public async Task<IReadOnlyList<Sale>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.CustomerId == customerId && s.DeletedAt == null)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.UserId == userId && s.DeletedAt == null)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Sale>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(s => s.CreatedAt >= startDate && s.CreatedAt <= endDate && s.DeletedAt == null)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<Sale?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sale = await GetByIdAsync(id, cancellationToken);

        if (sale == null)
            return null;

        await LoadSaleDetailsAsync(sale, cancellationToken);

        return sale;
    }

    public async Task<IReadOnlyList<Sale>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        var sales = await GetAllAsync(cancellationToken);

        foreach (var sale in sales)
        {
            await LoadSaleDetailsAsync(sale, cancellationToken);
        }

        return sales;
    }

    private async Task LoadSaleDetailsAsync(Sale sale, CancellationToken cancellationToken)
    {
        // Load Customer
        var customer = await _context.Set<Customer>()
            .FirstOrDefaultAsync(c => c.Id == sale.CustomerId && c.DeletedAt == null, cancellationToken);
        if (customer != null)
            sale.Customer = customer;

        // Load User
        var user = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == sale.UserId && u.DeletedAt == null, cancellationToken);
        if (user != null)
            sale.User = user;

        // Load SaleDetails
        var saleDetails = await _context.Set<SaleDetail>()
            .Where(sd => sd.SaleId == sale.Id)
            .ToListAsync(cancellationToken);

        sale.SaleDetails = saleDetails;

        // Load Product for each SaleDetail
        foreach (var detail in saleDetails)
        {
            var product = await _context.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == detail.ProductId && p.DeletedAt == null, cancellationToken);
            if (product != null)
                detail.Product = product;
        }
    }

    // ===============================
    // Dashboard & Analytics Methods
    // ===============================

    public async Task<IReadOnlyList<(int Hour, int SalesCount, decimal TotalAmount)>> GetHourlySalesAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var hourlyData = await _dbSet
            .Where(s => !s.IsCancelled && s.CreatedAt >= startDate && s.CreatedAt < endDate && s.DeletedAt == null)
            .AsNoTracking()
            .GroupBy(s => s.CreatedAt.Hour)
            .Select(g => new
            {
                Hour = g.Key,
                SalesCount = g.Count(),
                TotalAmount = g.Sum(s => s.TotalAmount)
            })
            .OrderBy(x => x.Hour)
            .ToListAsync(cancellationToken);

        return hourlyData
            .Select(h => (h.Hour, h.SalesCount, h.TotalAmount))
            .ToList();
    }

    public async Task<IReadOnlyList<(Guid ProductId, string ProductName, int Quantity, decimal Revenue)>> GetTopProductsAsync(
        DateTime startDate,
        DateTime endDate,
        int topCount = 10,
        CancellationToken cancellationToken = default)
    {
        var topProducts = await _dbSet
            .Where(s => !s.IsCancelled && s.CreatedAt >= startDate && s.CreatedAt < endDate && s.DeletedAt == null)
            .AsNoTracking()
            .AsSplitQuery()
            .SelectMany(s => s.SaleDetails)
            .GroupBy(sd => new { sd.ProductId, sd.Product.Name })
            .Select(g => new
            {
                ProductId = g.Key.ProductId,
                ProductName = g.Key.Name,
                Quantity = g.Sum(sd => sd.Quantity),
                Revenue = g.Sum(sd => sd.Total)
            })
            .OrderByDescending(x => x.Quantity)
            .Take(topCount)
            .ToListAsync(cancellationToken);

        return topProducts
            .Select(p => (p.ProductId, p.ProductName, p.Quantity, p.Revenue))
            .ToList();
    }

    public async Task<(int TotalSales, decimal TotalRevenue, decimal AvgTicketSize, int TotalItemsSold)> GetSalesSummaryAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        var sales = await _dbSet
            .Where(s => !s.IsCancelled && s.CreatedAt >= startDate && s.CreatedAt < endDate && s.DeletedAt == null)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        if (!sales.Any())
            return (0, 0, 0, 0);

        // Load sale details for total items calculation
        var saleIds = sales.Select(s => s.Id).ToList();
        var saleDetails = await _context.Set<SaleDetail>()
            .Where(sd => saleIds.Contains(sd.SaleId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalSales = sales.Count;
        var totalRevenue = sales.Sum(s => s.TotalAmount);
        var avgTicketSize = sales.Average(s => s.TotalAmount);
        var totalItemsSold = saleDetails.Sum(sd => sd.Quantity);

        return (totalSales, totalRevenue, avgTicketSize, totalItemsSold);
    }
}
