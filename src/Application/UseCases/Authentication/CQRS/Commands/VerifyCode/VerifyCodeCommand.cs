using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.UseCases.Authentication.DTOs;

namespace Application.UseCases.Authentication.CQRS.Commands.VerifyCode;

/// <summary>
/// Command to verify a password reset code.
/// </summary>
public record VerifyCodeCommand(string Email, string Code) : IRequest<OperationResult<VerifyCodeResponseDTO>>;
