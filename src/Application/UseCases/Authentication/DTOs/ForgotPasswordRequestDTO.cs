namespace Application.UseCases.Authentication.DTOs;

/// <summary>
/// Request DTO for forgot password operation.
/// </summary>
public record ForgotPasswordRequestDTO
{
    public string Email { get; init; } = string.Empty;
}
