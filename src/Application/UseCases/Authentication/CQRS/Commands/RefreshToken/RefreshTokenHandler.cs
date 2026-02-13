using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Application.Options;
using Application.UseCases.Authentication.DTOs;
using Domain.Entities.Users;
using Microsoft.Extensions.Options;

namespace Application.UseCases.Authentication.CQRS.Commands.RefreshToken;

/// <summary>
/// Handler for refreshing access tokens using a valid refresh token.
/// </summary>
public class RefreshTokenHandler(
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<RefreshTokenCommand, OperationResult<RefreshTokenResponseDTO>>
{
    public async Task<OperationResult<RefreshTokenResponseDTO>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validar input
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result.Error(ErrorResult.BadRequest, detail: "El refresh token es requerido.");

        // 2. Obtener refresh token activo
        var refreshToken = await unitOfWork.RefreshTokens.GetActiveTokenAsync(
            request.RefreshToken,
            cancellationToken);

        if (refreshToken == null || !refreshToken.IsActive)
            return Result.Error(ErrorResult.BadRequest, detail: UserMessages.Authentication.RefreshTokenInvalid);

        // 3. Obtener user con role
        var user = await unitOfWork.Users.GetByIdWithRoleAsync(refreshToken.UserId, cancellationToken);

        if (user == null)
            return Result.Error(ErrorResult.NotFound, detail: UserMessages.NotFound.General);

        // 4. Verificar si la cuenta está activa
        if (!user.IsActive)
            return Result.Error(ErrorResult.Forbidden, detail: UserMessages.Authentication.AccountInactive);

        // 5. Generar nuevo access token
        var accessToken = jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            user.RoleId,
            user.Role.Name);

        var expiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes);

        // 6. Retornar respuesta
        var response = new RefreshTokenResponseDTO
        {
            AccessToken = accessToken,
            ExpiresIn = jwtSettings.Value.AccessTokenExpirationMinutes * 60, // ExpiresIn en segundos
            AccessTokenExpiresAt = expiresAt
        };

        return Result.Success(response);
    }
}
