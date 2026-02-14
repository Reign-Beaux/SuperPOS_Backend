using System.Text.RegularExpressions;
using Application.DesignPatterns.Mediators.Interfaces;
using Application.DesignPatterns.OperationResults;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Application.UseCases.Authentication.DTOs;
using Domain.Entities.Security;
using Domain.Entities.Users;

namespace Application.UseCases.Authentication.CQRS.Commands.VerifyCode;

/// <summary>
/// Handler for verify password reset code command.
/// Validates the code and returns a verification token for the next step.
/// </summary>
public partial class VerifyCodeHandler(
    IUnitOfWork unitOfWork,
    ICurrentUserContext currentUserContext)
    : IRequestHandler<VerifyCodeCommand, OperationResult<VerifyCodeResponseDTO>>
{
    [GeneratedRegex(@"^\d{6}$")]
    private static partial Regex SixDigitRegex();

    public async Task<OperationResult<VerifyCodeResponseDTO>> Handle(
        VerifyCodeCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Validate email and code format
        if (string.IsNullOrWhiteSpace(request.Email))
            return Result.Error(ErrorResult.BadRequest, detail: "El email es requerido.");

        if (string.IsNullOrWhiteSpace(request.Code))
            return Result.Error(ErrorResult.BadRequest, detail: "El código es requerido.");

        // Validate code format (must be 6 digits)
        if (!SixDigitRegex().IsMatch(request.Code))
            return Result.Error(ErrorResult.BadRequest, detail: "El código debe ser de 6 dígitos.");

        // 2. Get user by email
        var user = await unitOfWork.Users.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
            return Result.Error(ErrorResult.NotFound, detail: UserMessages.PasswordReset.EmailNotFound);

        // 3. Get valid token via repository
        var resetToken = await unitOfWork.PasswordResetTokens.GetValidTokenByUserIdAsync(user.Id, cancellationToken);

        if (resetToken == null)
        {
            // Audit: Token not found
            var auditLogNotFound = SecurityAuditLog.Create(
                user.Id,
                SecurityAuditEventTypes.PasswordResetCodeInvalid,
                currentUserContext.IpAddress,
                currentUserContext.UserAgent,
                isSuccess: false,
                details: "No valid password reset token found");

            unitOfWork.Repository<SecurityAuditLog>().Add(auditLogNotFound);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Error(ErrorResult.NotFound, detail: UserMessages.PasswordReset.TokenNotFound);
        }

        // 4. Check if expired
        if (resetToken.IsExpired)
        {
            // Audit: Token expired
            var auditLogExpired = SecurityAuditLog.Create(
                user.Id,
                SecurityAuditEventTypes.PasswordResetCodeInvalid,
                currentUserContext.IpAddress,
                currentUserContext.UserAgent,
                isSuccess: false,
                details: "Password reset token expired");

            unitOfWork.Repository<SecurityAuditLog>().Add(auditLogExpired);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Error(ErrorResult.BadRequest, detail: UserMessages.PasswordReset.CodeExpired);
        }

        // 5. Check if already used
        if (resetToken.IsUsed)
        {
            // Audit: Token already used
            var auditLogUsed = SecurityAuditLog.Create(
                user.Id,
                SecurityAuditEventTypes.PasswordResetCodeInvalid,
                currentUserContext.IpAddress,
                currentUserContext.UserAgent,
                isSuccess: false,
                details: "Password reset token already used");

            unitOfWork.Repository<SecurityAuditLog>().Add(auditLogUsed);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Error(ErrorResult.BadRequest, detail: UserMessages.PasswordReset.TokenAlreadyUsed);
        }

        // 6. Validate code (increments AttemptCount)
        var isValidCode = resetToken.ValidateCode(request.Code);

        // Check if max attempts reached after validation
        if (resetToken.AttemptCount >= 3)
        {
            // Audit: Max attempts reached
            var auditLogMaxAttempts = SecurityAuditLog.Create(
                user.Id,
                SecurityAuditEventTypes.PasswordResetCodeInvalid,
                currentUserContext.IpAddress,
                currentUserContext.UserAgent,
                isSuccess: false,
                details: "Max password reset code validation attempts reached");

            unitOfWork.Repository<SecurityAuditLog>().Add(auditLogMaxAttempts);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Error(ErrorResult.BadRequest, detail: UserMessages.PasswordReset.MaxAttemptsReached);
        }

        if (!isValidCode)
        {
            // Audit: Invalid code
            var auditLogInvalid = SecurityAuditLog.Create(
                user.Id,
                SecurityAuditEventTypes.PasswordResetCodeInvalid,
                currentUserContext.IpAddress,
                currentUserContext.UserAgent,
                isSuccess: false,
                details: $"Invalid password reset code (attempt {resetToken.AttemptCount}/3)");

            unitOfWork.Repository<SecurityAuditLog>().Add(auditLogInvalid);
            await unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Error(ErrorResult.BadRequest, detail: UserMessages.PasswordReset.CodeInvalid);
        }

        // 7. Code is valid - Audit log success
        var auditLogSuccess = SecurityAuditLog.Create(
            user.Id,
            SecurityAuditEventTypes.PasswordResetCodeVerified,
            currentUserContext.IpAddress,
            currentUserContext.UserAgent,
            isSuccess: true,
            details: "Password reset code verified successfully");

        unitOfWork.Repository<SecurityAuditLog>().Add(auditLogSuccess);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        // 8. Return verification token (the reset token's Id) for next step
        var response = new VerifyCodeResponseDTO
        {
            VerificationToken = resetToken.Id.ToString(),
            Message = UserMessages.PasswordReset.CodeVerifiedSuccess
        };

        return Result.Success(response);
    }
}
