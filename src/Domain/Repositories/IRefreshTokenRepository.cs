using Domain.Entities.Authentication;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for RefreshToken entity with specialized authentication operations.
/// </summary>
public interface IRefreshTokenRepository : IRepositoryBase<RefreshToken>
{
    /// <summary>
    /// Gets an active (non-revoked, non-expired) refresh token by its token string.
    /// </summary>
    Task<RefreshToken?> GetActiveTokenAsync(string token, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all active refresh tokens for a specific user.
    /// </summary>
    Task<IReadOnlyList<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all active refresh tokens for a specific user.
    /// Useful for logout from all devices or security incidents.
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes expired tokens that are older than 7 days.
    /// Returns the count of deleted tokens for cleanup operations.
    /// </summary>
    Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}
