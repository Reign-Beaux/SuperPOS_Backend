namespace Application.UseCases.Authentication.DTOs;

/// <summary>
/// DTO for login request with user credentials.
/// </summary>
public record LoginRequestDTO(string Email, string Password);
