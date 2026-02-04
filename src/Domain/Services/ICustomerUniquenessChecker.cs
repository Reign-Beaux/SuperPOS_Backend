namespace Domain.Services;

/// <summary>
/// Domain service for checking customer uniqueness constraints.
/// Implemented in Infrastructure layer to access persistence.
/// </summary>
public interface ICustomerUniquenessChecker
{
    /// <summary>
    /// Checks if a customer email is unique in the system.
    /// </summary>
    /// <param name="email">Email to check</param>
    /// <param name="excludeId">Optional customer ID to exclude from the check (for updates)</param>
    Task<bool> IsEmailUniqueAsync(string email, Guid? excludeId = null, CancellationToken cancellationToken = default);
}
