namespace Application.UseCases.Authentication.DTOs;

/// <summary>
/// DTO for refresh token response with new access token and new refresh token (rotation).
/// </summary>
public record RefreshTokenResponseDTO
{
    public required string AccessToken { get; init; }
    public required int ExpiresIn { get; init; }
    public DateTime AccessTokenExpiresAt { get; init; }

    /// <summary>
    /// New refresh token (Token Rotation: old token is revoked, new one is generated).
    /// </summary>
    public required string RefreshToken { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
}
