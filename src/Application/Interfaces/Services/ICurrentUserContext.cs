namespace Application.Interfaces.Services;

/// <summary>
/// Provides information about the current HTTP request context.
/// Used for audit logging without coupling to ASP.NET Core.
/// </summary>
public interface ICurrentUserContext
{
    /// <summary>
    /// Gets the IP address of the current request.
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// Gets the User-Agent header of the current request.
    /// </summary>
    string? UserAgent { get; }
}
