namespace Domain.Entities.Security;

/// <summary>
/// Audit log for security-related events.
/// Tracks authentication attempts, authorization failures, and other security events.
/// </summary>
public class SecurityAuditLog : BaseEntity, IAggregateRoot
{
    private SecurityAuditLog() { } // EF Core

    public Guid? UserId { get; private set; }
    public string EventType { get; private set; } = string.Empty;
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? Details { get; private set; }
    public bool IsSuccess { get; private set; }

    /// <summary>
    /// Creates a security audit log entry.
    /// </summary>
    /// <param name="userId">User ID if applicable (null for anonymous events)</param>
    /// <param name="eventType">Type of event (Login, LoginFailed, Logout, etc.)</param>
    /// <param name="ipAddress">IP address of the request</param>
    /// <param name="userAgent">User agent string from the request</param>
    /// <param name="isSuccess">Whether the event was successful</param>
    /// <param name="details">Additional details about the event</param>
    public static SecurityAuditLog Create(
        Guid? userId,
        string eventType,
        string? ipAddress,
        string? userAgent,
        bool isSuccess,
        string? details = null)
    {
        return new SecurityAuditLog
        {
            Id = Guid.CreateVersion7(),
            UserId = userId,
            EventType = eventType,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            IsSuccess = isSuccess,
            Details = details,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}

/// <summary>
/// Constants for security audit event types.
/// </summary>
public static class SecurityAuditEventTypes
{
    public const string Login = "Login";
    public const string LoginFailed = "LoginFailed";
    public const string Logout = "Logout";
    public const string RefreshToken = "RefreshToken";
    public const string RefreshTokenFailed = "RefreshTokenFailed";
    public const string AccountLocked = "AccountLocked";
    public const string PasswordChanged = "PasswordChanged";
    public const string UserCreated = "UserCreated";
    public const string UserUpdated = "UserUpdated";
    public const string UserDeleted = "UserDeleted";
    public const string UnauthorizedAccess = "UnauthorizedAccess";
}
