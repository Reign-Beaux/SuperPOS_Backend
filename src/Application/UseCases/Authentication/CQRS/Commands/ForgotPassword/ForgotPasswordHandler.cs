using System.Security.Cryptography;
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Domain.Entities.Authentication;
using Domain.Entities.Security;
using Domain.Entities.Users;

namespace Application.UseCases.Authentication.CQRS.Commands.ForgotPassword;

/// <summary>
/// Handler for forgot password command.
/// Generates a 6-digit cryptographically secure code and sends it via email.
/// </summary>
public class ForgotPasswordHandler(
    IUnitOfWork unitOfWork,
    IEmailService emailService,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<ForgotPasswordCommand, OperationResult<string>>
{
    public async Task<OperationResult<string>> Handle(
        ForgotPasswordCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate email input
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Error(ErrorResult.BadRequest, detail: "El email es requerido.");

        // 2. Get user by email (if not found, return generic success for security - don't reveal email existence)
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
        {
            // Security: Don't reveal if email exists or not
            return Result.Success(UserMessages.PasswordReset.CodeSentSuccess);
        }

        // 3. Check if account is active
        if (!user.IsActive)
        {
            // Security: Don't reveal account status, return generic success
            return Result.Success(UserMessages.PasswordReset.CodeSentSuccess);
        }

        // 4. Revoke all previous password reset tokens for this user
        await unitOfWork.PasswordResetTokens.RevokeAllUserTokensAsync(user.Id, cancellationToken);

        // 5. Generate 6-digit cryptographically secure code
        var code = RandomNumberGenerator.GetInt32(0, 1000000).ToString("D6");

        // 6. Create PasswordResetToken entity
        var resetToken = PasswordResetToken.Create(user.Id, code);
        unitOfWork.PasswordResetTokens.Add(resetToken);

        // 7. Send email with code
        var emailSent = await emailService.SendPasswordResetCodeAsync(
            user.Email,
            user.GetFullName(),
            code,
            cancellationToken);

        if (!emailSent)
        {
            // Audit: Email send failed
            var auditLogFailed = SecurityAuditLog.Create(
                user.Id,
                SecurityAuditEventTypes.PasswordResetRequested,
                currentUserContext.IpAddress,
                currentUserContext.UserAgent,
                isSuccess: false,
                details: "Failed to send password reset email");

            unitOfWork.Repository<SecurityAuditLog>().Add(auditLogFailed);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Error(ErrorResult.InternalServerError, detail: UserMessages.PasswordReset.CodeSentFailed);
        }

        // 8. Audit log: Password reset requested
        var auditLogSuccess = SecurityAuditLog.Create(
            user.Id,
            SecurityAuditEventTypes.PasswordResetRequested,
            currentUserContext.IpAddress,
            currentUserContext.UserAgent,
            isSuccess: true,
            details: $"Password reset code sent to {user.Email}");

        unitOfWork.Repository<SecurityAuditLog>().Add(auditLogSuccess);

        // 9. Save changes (reset token revocation, new token, audit log)
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 10. Return success message
        return Result.Success(UserMessages.PasswordReset.CodeSentSuccess);
    }
}
