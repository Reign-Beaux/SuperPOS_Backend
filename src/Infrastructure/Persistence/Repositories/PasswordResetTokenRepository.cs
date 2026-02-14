using Domain.Entities.Authentication;
using Domain.Repositories;
using Infrastructure.Persistence.Context;

namespace Infrastructure.Persistence.Repositories;

/// <summary>
/// Implementation of IPasswordResetTokenRepository with specialized password reset operations.
/// </summary>
public sealed class PasswordResetTokenRepository : RepositoryBase<PasswordResetToken>, IPasswordResetTokenRepository
{
    public PasswordResetTokenRepository(SuperPOSDbContext context) : base(context) { }

    public async Task<PasswordResetToken?> GetValidTokenByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet
            .Where(prt => prt.UserId == userId && !prt.IsUsed && prt.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(prt => prt.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<PasswordResetToken?> GetByCodeAndUserIdAsync(string code, Guid userId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        return await _dbSet
            .Where(prt => prt.Code == code && prt.UserId == userId)
            .OrderByDescending(prt => prt.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var activeTokens = await _dbSet
            .Where(prt => prt.UserId == userId && !prt.IsUsed)
            .ToListAsync(cancellationToken);

        foreach (var token in activeTokens)
        {
            if (token.IsValid)
            {
                token.MarkAsUsed();
            }
        }
    }

    public async Task<int> DeleteExpiredTokensAsync(CancellationToken cancellationToken = default)
    {
        var expiredTokens = await _dbSet
            .Where(prt => prt.ExpiresAt < DateTime.UtcNow.AddDays(-7))
            .ToListAsync(cancellationToken);

        _dbSet.RemoveRange(expiredTokens);
        return expiredTokens.Count;
    }
}
