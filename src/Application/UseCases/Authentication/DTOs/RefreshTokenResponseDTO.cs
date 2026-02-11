namespace Application.UseCases.Authentication.DTOs;

/// <summary>
/// DTO for refresh token response with new access token.
/// </summary>
public record RefreshTokenResponseDTO(
    string AccessToken,
    DateTime AccessTokenExpiresAt
);
