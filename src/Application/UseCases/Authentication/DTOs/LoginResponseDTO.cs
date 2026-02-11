using Application.UseCases.Users.DTOs;

namespace Application.UseCases.Authentication.DTOs;

/// <summary>
/// DTO for successful login response with tokens and user information.
/// </summary>
public record LoginResponseDTO(
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiresAt,
    DateTime RefreshTokenExpiresAt,
    UserDTO User
);
