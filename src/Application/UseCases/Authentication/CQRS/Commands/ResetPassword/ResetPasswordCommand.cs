using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Authentication.CQRS.Commands.ResetPassword;

/// <summary>
/// Command to reset password using a verification token.
/// </summary>
public record ResetPasswordCommand(string VerificationToken, string NewPassword) : IRequest<OperationResult<string>>;
