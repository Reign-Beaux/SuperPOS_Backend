using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Application.Options;
using Application.UseCases.Authentication.DTOs;
using Application.UseCases.Users.DTOs;
using Domain.Entities.Users;
using Microsoft.Extensions.Options;
using RefreshTokenEntity = Domain.Entities.Authentication.RefreshToken;

namespace Application.UseCases.Authentication.CQRS.Commands.Login;

/// <summary>
/// Handler for user login with JWT token generation.
/// </summary>
public class LoginHandler(
    IUnitOfWork unitOfWork,
    IEncryptionService encryptionService,
    IJwtTokenService jwtTokenService,
    IMapper mapper,
    IOptions<JwtSettings> jwtSettings)
    : IRequestHandler<LoginCommand, OperationResult<LoginResponseDTO>>
{
    public async Task<OperationResult<LoginResponseDTO>> Handle(
        LoginCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validar input
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Error(ErrorResult.BadRequest, detail: "El email es requerido.");

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result.Error(ErrorResult.BadRequest, detail: "La contraseña es requerida.");

        // 2. Obtener user con role
        var user = await unitOfWork.Users.GetByEmailWithRoleAsync(request.Email, cancellationToken);

        if (user == null)
            return Result.Error(ErrorResult.BadRequest, detail: UserMessages.Authentication.InvalidCredentials);

        // 3. Verificar si la cuenta está bloqueada
        if (user.IsLocked)
        {
            var remainingMinutes = (int)(user.LockedUntilAt!.Value - DateTime.UtcNow).TotalMinutes + 1;
            return Result.Error(
                ErrorResult.Forbidden,
                detail: string.Format(UserMessages.Authentication.AccountLocked, remainingMinutes));
        }

        // 4. Verificar si la cuenta está activa
        if (!user.IsActive)
            return Result.Error(ErrorResult.Forbidden, detail: UserMessages.Authentication.AccountInactive);

        // 5. Validar password con BCrypt
        var isValidPassword = encryptionService.VerifyText(request.Password, user.PasswordHashed);

        if (!isValidPassword)
        {
            // 6. Registrar intento fallido
            user.RecordFailedLogin();
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Error(ErrorResult.BadRequest, detail: UserMessages.Authentication.InvalidCredentials);
        }

        // 7. Registrar login exitoso
        user.RecordSuccessfulLogin();

        // 8. Generar access token
        var accessToken = jwtTokenService.GenerateAccessToken(
            user.Id,
            user.Email,
            user.RoleId,
            user.Role.Name);

        // 9. Generar refresh token
        var refreshTokenString = jwtTokenService.GenerateRefreshToken();

        // 10. Crear RefreshToken entity
        var refreshToken = RefreshTokenEntity.Create(
            user.Id,
            refreshTokenString,
            jwtSettings.Value.RefreshTokenExpirationDays);

        unitOfWork.RefreshTokens.Add(refreshToken);

        // Guardar cambios (login exitoso y refresh token)
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 11. Mapear user a DTO
        var userDto = mapper.Map<UserDTO>(user);

        // 12. Retornar respuesta
        var response = new LoginResponseDTO
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenString,
            ExpiresIn = jwtSettings.Value.AccessTokenExpirationMinutes * 60, // ExpiresIn en segundos
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(jwtSettings.Value.AccessTokenExpirationMinutes),
            RefreshTokenExpiresAt = refreshToken.ExpiresAt,
            User = userDto
        };

        return Result.Success(response);
    }
}
