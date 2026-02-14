namespace Application.UseCases.Authentication.DTOs;

/// <summary>
/// Response DTO for verify password reset code operation.
/// </summary>
public record VerifyCodeResponseDTO
{
    public string VerificationToken { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}
