using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;

namespace Application.UseCases.Authentication.CQRS.Commands.Logout;

/// <summary>
/// Command to logout a user by revoking their refresh token.
/// </summary>
public record LogoutCommand(string RefreshToken)
    : IRequest<OperationResult<VoidResult>>;
