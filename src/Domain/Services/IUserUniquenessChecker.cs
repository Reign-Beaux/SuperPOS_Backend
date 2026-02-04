namespace Domain.Services;

/// <summary>
/// Domain service for checking user uniqueness constraints.
/// Implemented in Infrastructure layer to access persistence.
/// </summary>
public interface IUserUniquenessChecker
{
    /// <summary>
    /// Checks if a user email is unique in the system.
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeId">Optional user ID to exclude from the check (for updates)</param>
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
