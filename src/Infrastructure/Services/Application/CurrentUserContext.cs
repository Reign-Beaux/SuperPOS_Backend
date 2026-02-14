using Application.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.Application;

/// <summary>
/// Provides information about the current HTTP request context.
/// </summary>
public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent => _httpContextAccessor.HttpContext?.Request.Headers["User-Agent"].ToString();
}
