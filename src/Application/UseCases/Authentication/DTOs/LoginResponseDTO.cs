using Application.UseCases.Users.DTOs;

namespace Application.UseCases.Authentication.DTOs;

/// <summary>
/// DTO for successful login response with tokens and user information.
/// </summary>
public record LoginResponseDTO
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public DateTime AccessTokenExpiresAt { get; init; }
    public DateTime RefreshTokenExpiresAt { get; init; }
    public required UserDTO User { get; init; }
}
