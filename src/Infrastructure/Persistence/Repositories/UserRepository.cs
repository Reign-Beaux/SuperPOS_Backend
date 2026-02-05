using Domain.Entities.Roles;
using Domain.Entities.Users;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Repository implementation for User aggregate root.
/// </summary>
public sealed class UserRepository : RepositoryBase<User>, IUserRepository
{
    public UserRepository(SuperPOSDbContext context) : base(context)
    {
    }

    public async Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        var normalizedEmail = email.ToLower();
        var query = _dbSet.Where(u => u.Email.ToLower() == normalizedEmail && u.DeletedAt == null);

        if (excludeId.HasValue)
        {
            query = query.Where(u => u.Id != excludeId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.ToLower();

        return await _dbSet
            .Where(u => u.Email.ToLower() == normalizedEmail && u.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<User?> GetByEmailWithRoleAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.ToLower();

        var user = await _dbSet
            .Where(u => u.Email.ToLower() == normalizedEmail && u.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        if (user != null)
        {
            user.Role = (await _context.Set<Role>()
                .FirstOrDefaultAsync(r => r.Id == user.RoleId && r.DeletedAt == null, cancellationToken))!;
        }

        return user;
    }

    public async Task<User?> GetByIdWithRoleAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);

        if (user != null)
        {
            user.Role = (await _context.Set<Role>()
                .FirstOrDefaultAsync(r => r.Id == user.RoleId && r.DeletedAt == null, cancellationToken))!;
        }

        return user;
    }
}
