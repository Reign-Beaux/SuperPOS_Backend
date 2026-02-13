namespace Application.UseCases.Authentication.DTOs;

/// <summary>
/// DTO for refresh token response with new access token.
/// </summary>
public record RefreshTokenResponseDTO
{
    public required string AccessToken { get; init; }
    public required int ExpiresIn { get; init; }
    public DateTime AccessTokenExpiresAt { get; init; }
}
