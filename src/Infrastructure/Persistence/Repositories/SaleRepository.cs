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
        sale.Customer = await _context.Set<Customer>()
            .FirstOrDefaultAsync(c => c.Id == sale.CustomerId && c.DeletedAt == null, cancellationToken);

        // Load User
        sale.User = await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Id == sale.UserId && u.DeletedAt == null, cancellationToken);

        // Load SaleDetails
        var saleDetails = await _context.Set<SaleDetail>()
            .Where(sd => sd.SaleId == sale.Id)
            .ToListAsync(cancellationToken);

        sale.SaleDetails = saleDetails;

        // Load Product for each SaleDetail
        foreach (var detail in saleDetails)
        {
            detail.Product = await _context.Set<Product>()
                .FirstOrDefaultAsync(p => p.Id == detail.ProductId && p.DeletedAt == null, cancellationToken);
        }
    }
}
