using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Domain.Entities.Users;

namespace Application.UseCases.Authentication.CQRS.Commands.Logout;

/// <summary>
/// Handler for user logout by revoking refresh token.
/// Idempotent operation - safe to call multiple times.
/// </summary>
public class LogoutHandler(IUnitOfWork unitOfWork)
    : IRequestHandler<LogoutCommand, OperationResult<VoidResult>>
{
    public async Task<OperationResult<VoidResult>> Handle(
        LogoutCommand request,
        CancellationToken cancellationToken)
    {
        // Validar input
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
            return Result.Error(ErrorResult.BadRequest, detail: "El refresh token es requerido.");

        // 1. Buscar refresh token (incluso si está revocado o expirado)
        var refreshToken = await unitOfWork.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == request.RefreshToken, cancellationToken);

        // 2. Si existe y no está revocado, revocarlo
        if (refreshToken != null && !refreshToken.IsRevoked)
        {
            refreshToken.Revoke();
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        // 3. Retornar success (idempotente - siempre exitoso)
        return Result.Success(new VoidResult());
    }
}
