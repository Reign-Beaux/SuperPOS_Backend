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

    public async Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalizedEmail = email.ToLower();

        var user = await _dbSet
            .Where(u => u.Email.ToLower() == normalizedEmail && u.DeletedAt == null)
            .FirstOrDefaultAsync(cancellationToken);

        if (user != null)
        {
            await LoadUserRolesAsync(user, cancellationToken);
        }

        return user;
    }

    public async Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = await GetByIdAsync(id, cancellationToken);

        if (user != null)
        {
            await LoadUserRolesAsync(user, cancellationToken);
        }

        return user;
    }

    private async Task LoadUserRolesAsync(User user, CancellationToken cancellationToken)
    {
        // Load UserRoles
        var userRoles = await _context.Set<UserRole>()
            .Where(ur => ur.UserId == user.Id)
            .ToListAsync(cancellationToken);

        user.UserRoles = userRoles;

        // Load Role entities for each UserRole
        foreach (var userRole in userRoles)
        {
            userRole.Role = (await _context.Set<Role>()
                .FirstOrDefaultAsync(r => r.Id == userRole.RoleId && r.DeletedAt == null, cancellationToken))!;
        }
    }
}
