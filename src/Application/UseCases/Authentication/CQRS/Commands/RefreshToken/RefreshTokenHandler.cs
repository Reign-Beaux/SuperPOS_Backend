using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Application.Options;
using Application.UseCases.Authentication.DTOs;
using Domain.Entities.Authentication;
using Domain.Entities.Users;
using Microsoft.Extensions.Options;

namespace Application.UseCases.Authentication.CQRS.Commands.RefreshToken;

/// <summary>
/// Handler for refreshing access tokens using a valid refresh token.
/// Implements Refresh Token Rotation: generates a new refresh token and revokes the old one.
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

        var accessTokenExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes);

        // 6. REFRESH TOKEN ROTATION - Generar nuevo refresh token
        var newRefreshTokenValue = jwtTokenService.GenerateRefreshToken();

        var newRefreshToken = Domain.Entities.Authentication.RefreshToken.Create(
            user.Id,
            newRefreshTokenValue,
            jwtSettings.Value.RefreshTokenExpirationDays);

        var refreshTokenExpiresAt = newRefreshToken.ExpiresAt;

        // 7. Revocar el refresh token anterior
        refreshToken.Revoke();

        // 8. Guardar el nuevo refresh token
        unitOfWork.RefreshTokens.Add(newRefreshToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 9. Retornar respuesta con nuevo access token y nuevo refresh token
        var response = new RefreshTokenResponseDTO
        {
            AccessToken = accessToken,
            ExpiresIn = jwtSettings.Value.AccessTokenExpirationMinutes * 60, // ExpiresIn en segundos
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshToken = newRefreshTokenValue,
            RefreshTokenExpiresAt = refreshTokenExpiresAt
        };

        return Result.Success(response);
    }
}
