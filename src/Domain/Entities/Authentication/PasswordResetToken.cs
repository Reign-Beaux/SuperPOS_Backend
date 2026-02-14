using Domain.Exceptions;

namespace Domain.Entities.Authentication;

/// <summary>
/// Password reset token for account recovery.
/// Contains temporary verification codes sent via email or WhatsApp.
/// </summary>
public class PasswordResetToken : BaseEntity, IAggregateRoot
{
    private const int MaxAttempts = 3;
    private const int ExpirationMinutes = 15;

    // Parameterless constructor for EF Core
    public PasswordResetToken() { }

    public Guid UserId { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public int AttemptCount { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }

    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsValid => !IsUsed && !IsExpired && AttemptCount < MaxAttempts;

    /// <summary>
    /// Factory method to create a new password reset token.
    /// </summary>
    public static PasswordResetToken Create(Guid userId, string code)
    {
        return new PasswordResetToken
        {
            UserId = userId,
            Code = code,
            ExpiresAt = DateTime.UtcNow.AddMinutes(ExpirationMinutes),
            AttemptCount = 0,
            IsUsed = false
        };
    }

    /// <summary>
    /// Validates the provided code.
    /// </summary>
    public bool ValidateCode(string code)
    {
        if (!IsValid)
            return false;

        AttemptCount++;

        if (Code != code)
            return false;

        return true;
    }

    /// <summary>
    /// Marks the token as used.
    /// </summary>
    public void MarkAsUsed()
    {
        if (IsUsed)
            throw new BusinessRuleViolationException("TOKEN_001", "Token already used");

        if (!IsValid)
            throw new BusinessRuleViolationException("TOKEN_002", "Token is not valid");

        IsUsed = true;
        UsedAt = DateTime.UtcNow;
    }
}
