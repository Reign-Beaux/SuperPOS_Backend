using Domain.Entities.Roles;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for Role aggregate root.
/// Provides specific operations for role management.
/// </summary>
public interface IRoleRepository : IRepositoryBase<Role>
{
    /// <summary>
    /// Gets a role by its name.
    /// </summary>
    Task<Role?> GetByNameAsync(string name, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a role with the given name already exists (case-insensitive).
    /// </summary>
    /// <param name="name">Role name to check</param>
    /// <param name="excludeId">Optional role ID to exclude from the check (for updates)</param>
    Task<bool> ExistsByNameAsync(string name, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
