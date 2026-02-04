using Domain.Entities.Roles;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for Role aggregate root.
/// </summary>
public sealed class RoleRepository : RepositoryBase<Role>, IRoleRepository
{
    public RoleRepository(SuperPOSDbContext context) : base(context)
    {
    }

    public async Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return null;

        var normalizedName = name.ToLower();

        return await _dbSet
            .Where(r => r.Name.ToLower() == normalizedName && r.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;

        var normalizedName = name.ToLower();
        var query = _dbSet.Where(r => r.Name.ToLower() == normalizedName && r.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }
}
