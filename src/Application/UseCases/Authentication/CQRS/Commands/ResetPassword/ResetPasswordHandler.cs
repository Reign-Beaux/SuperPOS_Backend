using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities.Security;
using Domain.Entities.Users;
using Domain.Exceptions;
using Domain.ValueObjects;

namespace Application.UseCases.Authentication.CQRS.Commands.ResetPassword;

/// <summary>
/// Handler for reset password command.
/// Changes the user's password using a verified token and revokes all active sessions.
/// </summary>
public class ResetPasswordHandler(
    IUnitOfWork unitOfWork,
    IEncryptionService encryptionService,
    IEmailService emailService,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<ResetPasswordCommand, OperationResult<string>>
{
    public async Task<OperationResult<string>> Handle(
        ResetPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate verification token (must be valid Guid)
        if (string.IsNullOrWhiteSpace(request.VerificationToken))
            return Result.Error(ErrorResult.BadRequest, detail: "El token de verificación es requerido.");

        if (!Guid.TryParse(request.VerificationToken, out var tokenId))
            return Result.Error(ErrorResult.BadRequest, detail: "El token de verificación no es válido.");

        // 2. Get PasswordResetToken by ID
        var resetToken = await unitOfWork.PasswordResetTokens.GetByIdAsync(tokenId, cancellationToken);

        if (resetToken == null)
            return Result.Error(ErrorResult.NotFound, detail: UserMessages.PasswordReset.TokenNotFound);

        // 3. Check token is not used
        if (resetToken.IsUsed)
            return Result.Error(ErrorResult.BadRequest, detail: UserMessages.PasswordReset.TokenAlreadyUsed);

        // 4. Check token is not expired
        if (resetToken.IsExpired)
            return Result.Error(ErrorResult.BadRequest, detail: UserMessages.PasswordReset.CodeExpired);

        // 5. Check token is valid (not used, not expired, attempt count < 3)
        if (!resetToken.IsValid)
            return Result.Error(ErrorResult.BadRequest, detail: UserMessages.PasswordReset.TokenNotFound);

        // 6. Validate new password complexity using Password value object
        try
        {
            var passwordVO = Password.Create(request.NewPassword);
        }
        catch (InvalidValueObjectException ex)
        {
            // Password doesn't meet complexity requirements
            return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
        }

        // 7. Get user
        var user = await unitOfWork.Users.GetByIdAsync(resetToken.UserId, cancellationToken);

        if (user == null)
            return Result.Error(ErrorResult.NotFound, detail: UserMessages.NotFound.General);

        // 8. Hash new password with BCrypt
        var hashedPassword = encryptionService.HashText(request.NewPassword);

        // 9. Change password
        try
        {
            user.ChangePassword(hashedPassword);
        }
        catch (BusinessRuleViolationException ex)
        {
            return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
        }

        // 10. Mark token as used
        try
        {
            resetToken.MarkAsUsed();
        }
        catch (BusinessRuleViolationException ex)
        {
            return Result.Error(ErrorResult.BadRequest, detail: ex.Message);
        }

        // 11. Revoke all RefreshTokens (force re-login on all devices)
        await unitOfWork.RefreshTokens.RevokeAllUserTokensAsync(user.Id, cancellationToken);

        // 12. Audit log: Password reset completed
        var auditLog = SecurityAuditLog.Create(
            user.Id,
            SecurityAuditEventTypes.PasswordResetCompleted,
            currentUserContext.IpAddress,
            currentUserContext.UserAgent,
            isSuccess: true,
            details: $"Password reset completed for {user.Email}");

        unitOfWork.Repository<SecurityAuditLog>().Add(auditLog);

        // 13. Save changes (password, token used, refresh tokens revoked, audit log)
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 14. Send password changed notification email (fire and forget, don't block on failure)
        _ = emailService.SendPasswordChangedNotificationAsync(
            user.Email,
            user.GetFullName(),
            cancellationToken);

        // 15. Return success message
        return Result.Success(UserMessages.PasswordReset.ResetSuccess);
    }
}
