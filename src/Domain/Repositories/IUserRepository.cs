using Domain.Entities.Users;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for User aggregate root.
/// Provides specific operations for user management.
/// </summary>
public interface IUserRepository : IRepositoryBase<User>
{
    /// <summary>
    /// Checks if a user with the given email already exists.
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeId">Optional user ID to exclude from the check (for updates)</param>
    Task<bool> ExistsByEmailAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by their email address.
    /// </summary>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by email with their associated roles loaded.
    /// </summary>
    Task<User?> GetByEmailWithRolesAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID with their associated roles loaded.
    /// </summary>
    Task<User?> GetByIdWithRolesAsync(Guid id, CancellationToken cancellationToken = default);
}
