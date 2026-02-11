using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Authentication.CQRS.Commands.Login;
using Application.UseCases.Authentication.CQRS.Commands.RefreshToken;
using Application.UseCases.Authentication.CQRS.Commands.Logout;
using Application.UseCases.Authentication.DTOs;

namespace Web.API.Controllers;

/// <summary>
/// Authentication controller for login, token refresh, and logout operations.
/// </summary>
[Route("api/[controller]")]
public class AuthController(IMediator mediator) : BaseController
{
    /// <summary>
    /// Authenticates a user with email and password.
    /// Returns access token and refresh token on success.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDTO request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Refreshes an expired access token using a valid refresh token.
    /// Returns a new access token.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDTO request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Logs out a user by revoking their refresh token.
    /// Idempotent operation - safe to call multiple times.
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDTO request)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var result = await mediator.Send(command);
        return HandleResult(result);
    }
}
