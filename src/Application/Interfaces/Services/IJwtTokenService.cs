using System.Security.Claims;

namespace Application.Interfaces.Services;

/// <summary>
/// Service for JWT token generation and validation.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a new access token (JWT) with user claims.
    /// </summary>
    string GenerateAccessToken(Guid userId, string email, Guid roleId, string roleName);

    /// <summary>
    /// Generates a cryptographically secure refresh token.
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Validates a JWT token and returns the ClaimsPrincipal if valid.
    /// Returns null if token is invalid or expired.
    /// </summary>
    ClaimsPrincipal? ValidateToken(string token);

    /// <summary>
    /// Extracts the user ID from a JWT token without full validation.
    /// Returns null if token cannot be parsed or user ID is invalid.
    /// </summary>
    Guid? GetUserIdFromToken(string token);
}
