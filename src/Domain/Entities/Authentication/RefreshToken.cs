namespace Domain.Entities.Authentication;

/// <summary>
/// Refresh token for JWT authentication.
/// Allows users to obtain new access tokens without re-authenticating.
/// </summary>
public class RefreshToken : BaseEntity, IAggregateRoot
{
    // Parameterless constructor for EF Core
    public RefreshToken() { }

    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public bool IsRevoked => RevokedAt.HasValue;
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsActive => !IsRevoked && !IsExpired;

    /// <summary>
    /// Factory method to create a new refresh token.
    /// </summary>
    public static RefreshToken Create(
        Guid userId,
        string token,
        int expirationDays = 30)
    {
        return new RefreshToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddDays(expirationDays)
        };
    }

    /// <summary>
    /// Revokes this refresh token.
    /// </summary>
    public void Revoke()
    {
        if (IsRevoked)
            return;

        RevokedAt = DateTime.UtcNow;
    }
}
