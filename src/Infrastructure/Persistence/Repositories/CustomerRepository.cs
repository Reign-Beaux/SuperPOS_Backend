using Domain.Entities.Customers;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Customer aggregate root.
/// </summary>
public sealed class CustomerRepository : RepositoryBase<Customer>, ICustomerRepository
{
    public CustomerRepository(SuperPOSDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalizedEmail = email.ToLower();
        var query = _dbSet.Where(c => c.Email != null && c.Email.ToLower() == normalizedEmail && c.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(c => c.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.ToLower();

        return await _dbSet
            .Where(c => c.Email != null && c.Email.ToLower() == normalizedEmail && c.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> SearchByNameAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return await GetAllAsync(cancellationToken);

        var normalizedTerm = searchTerm.ToLower();

        return await _dbSet
            .Where(c => c.DeletedAt == null &&
                       (c.Name.ToLower().Contains(normalizedTerm) ||
                        c.FirstLastname.ToLower().Contains(normalizedTerm) ||
                        (c.SecondLastname != null && c.SecondLastname.ToLower().Contains(normalizedTerm))))
            .OrderBy(c => c.Name)
            .ThenBy(c => c.FirstLastname)
            .ToListAsync(cancellationToken);
    }
}
