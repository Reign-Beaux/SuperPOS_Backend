using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Authentication.CQRS.Commands.ForgotPassword;

/// <summary>
/// Command to request a password reset code.
/// </summary>
public record ForgotPasswordCommand(string Email) : IRequest<OperationResult<string>>;
