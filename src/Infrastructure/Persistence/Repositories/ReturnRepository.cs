using Domain.Entities.Returns;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

public class ReturnRepository : RepositoryBase<Return>, IReturnRepository
{
    public ReturnRepository(SuperPOSDbContext context) : base(context)
    {
    }

    public async Task<Return?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Returns
            .Include(r => r.Sale)
            .Include(r => r.Customer)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.ReturnDetails)
                .ThenInclude(rd => rd.Product)
            .FirstOrDefaultAsync(r => r.Id == id && r.DeletedAt == null, cancellationToken);
    }

    public async Task<List<Return>> GetBySaleIdAsync(Guid saleId, CancellationToken cancellationToken = default)
    {
        return await _context.Returns
            .Include(r => r.ReturnDetails)
            .Where(r => r.SaleId == saleId && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Return>> GetByStatusAsync(ReturnStatus status, CancellationToken cancellationToken = default)
    {
        return await _context.Returns
            .Include(r => r.Sale)
            .Include(r => r.Customer)
            .Include(r => r.ProcessedByUser)
            .Include(r => r.ReturnDetails)
            .Where(r => r.Status == status && r.DeletedAt == null)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync(cancellationToken);
    }
}
