using Application.DesignPatterns.Mediators.Interfaces;
using Application.UseCases.Authentication.CQRS.Commands.Login;
using Application.UseCases.Authentication.CQRS.Commands.RefreshToken;
using Application.UseCases.Authentication.CQRS.Commands.Logout;
using Application.UseCases.Authentication.CQRS.Commands.ForgotPassword;
using Application.UseCases.Authentication.CQRS.Commands.VerifyCode;
using Application.UseCases.Authentication.CQRS.Commands.ResetPassword;
using Application.UseCases.Authentication.DTOs;
using Microsoft.AspNetCore.Authorization;

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

    /// <summary>
    /// Requests a password reset code to be sent to the user's email.
    /// Returns success message regardless of email existence (security).
    /// </summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Verifies a password reset code sent to the user's email.
    /// Returns a verification token for the next step on success.
    /// </summary>
    [HttpPost("verify-code")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCode([FromBody] VerifyCodeRequestDTO request)
    {
        var command = new VerifyCodeCommand(request.Email, request.Code);
        var result = await mediator.Send(command);
        return HandleResult(result);
    }

    /// <summary>
    /// Resets the user's password using a verification token.
    /// Revokes all active sessions and sends confirmation email.
    /// </summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
    {
        var command = new ResetPasswordCommand(request.VerificationToken, request.NewPassword);
        var result = await mediator.Send(command);
        return HandleResult(result);
    }
}
