namespace Application.UseCases.Authentication.DTOs;

/// <summary>
/// Request DTO for verify password reset code operation.
/// </summary>
public record VerifyCodeRequestDTO
{
    public string Email { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
}
