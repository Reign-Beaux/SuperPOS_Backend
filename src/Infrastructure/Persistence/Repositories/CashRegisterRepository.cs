using Domain.Entities.CashRegisters;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

public class CashRegisterRepository : RepositoryBase<CashRegister>, ICashRegisterRepository
{
    public CashRegisterRepository(SuperPOSDbContext context) : base(context)
    {
    }

    public async Task<CashRegister?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.CashRegisters
            .Include(c => c.User)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<CashRegister>> GetAllWithDetailsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CashRegisters
            .Include(c => c.User)
            .OrderByDescending(c => c.ClosingDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CashRegister>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.CashRegisters
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.ClosingDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<CashRegister>> GetByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _context.CashRegisters
            .Include(c => c.User)
            .Where(c => c.ClosingDate >= startDate && c.ClosingDate <= endDate)
            .OrderByDescending(c => c.ClosingDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<CashRegister?> GetLastClosingAsync(CancellationToken cancellationToken = default)
    {
        return await _context.CashRegisters
            .Include(c => c.User)
            .OrderByDescending(c => c.ClosingDate)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
