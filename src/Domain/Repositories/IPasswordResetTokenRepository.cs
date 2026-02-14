using Domain.Entities.Authentication;

namespace Domain.Repositories;

/// <summary>
/// Repository interface for PasswordResetToken entity with specialized password reset operations.
/// </summary>
public interface IPasswordResetTokenRepository : IRepositoryBase<PasswordResetToken>
{
    /// <summary>
    /// Gets the most recent valid (non-used, non-expired) password reset token for a user.
    /// </summary>
    Task<PasswordResetToken?> GetValidTokenByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a password reset token by code and user ID.
    /// </summary>
    Task<PasswordResetToken?> GetByCodeAndUserIdAsync(string code, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Revokes all active password reset tokens for a specific user by marking them as used.
    /// Useful when generating a new reset code or after successful password change.
    /// </summary>
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes expired tokens that are older than 7 days.
    /// Returns the count of deleted tokens for cleanup operations.
    /// </summary>
    Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default);
}
