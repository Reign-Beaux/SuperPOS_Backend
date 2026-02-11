using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Authentication.DTOs;

namespace Application.UseCases.Authentication.CQRS.Commands.RefreshToken;

/// <summary>
/// Command to refresh an access token using a valid refresh token.
/// </summary>
public record RefreshTokenCommand(string RefreshToken)
    : IRequest<OperationResult<RefreshTokenResponseDTO>>;
