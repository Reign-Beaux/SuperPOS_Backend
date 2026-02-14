namespace Application.Interfaces.Services;

/// <summary>
/// Service for sending emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Sends a low stock alert email to specified recipient.
    /// </summary>
    Task<bool> SendLowStockAlertAsync(
        string recipientEmail,
        string productName,
        string? barcode,
        int currentStock,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a generic email.
    /// </summary>
    Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        string emailType,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password reset code email to the user.
    /// </summary>
    Task<bool> SendPasswordResetCodeAsync(
        string recipientEmail,
        string userName,
        string code,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a password changed notification email to the user.
    /// </summary>
    Task<bool> SendPasswordChangedNotificationAsync(
        string recipientEmail,
        string userName,
        CancellationToken cancellationToken = default);
}
