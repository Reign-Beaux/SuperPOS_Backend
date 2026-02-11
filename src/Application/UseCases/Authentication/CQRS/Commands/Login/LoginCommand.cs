using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Authentication.DTOs;

namespace Application.UseCases.Authentication.CQRS.Commands.Login;

/// <summary>
/// Command to authenticate a user with email and password.
/// </summary>
public record LoginCommand(string Email, string Password)
    : IRequest<OperationResult<LoginResponseDTO>>;
