namespace Application.UseCases.Authentication.DTOs;

/// <summary>
/// Request DTO for reset password operation.
/// </summary>
public record ResetPasswordRequestDTO
{
    public string VerificationToken { get; init; } = string.Empty;
    public string NewPassword { get; init; } = string.Empty;
}
