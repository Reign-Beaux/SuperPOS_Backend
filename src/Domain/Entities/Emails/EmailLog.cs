namespace Domain.Entities.Emails;

/// <summary>
/// Email log for auditing all emails sent by the system.
/// Tracks stock alerts, password recovery, and other notifications.
/// </summary>
public class EmailLog : BaseEntity, IAggregateRoot
{
    // Parameterless constructor for EF Core
    public EmailLog() { }

    public string Recipient { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsSent { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? SentAt { get; set; }
    public string EmailType { get; set; } = string.Empty; // StockAlert, PasswordRecovery, etc.

    /// <summary>
    /// Factory method to create a new email log.
    /// </summary>
    public static EmailLog Create(
        string recipient,
        string subject,
        string body,
        string emailType)
    {
        return new EmailLog
        {
            Recipient = recipient,
            Subject = subject,
            Body = body,
            EmailType = emailType,
            IsSent = false
        };
    }

    /// <summary>
    /// Marks the email as sent successfully.
    /// </summary>
    public void MarkAsSent()
    {
        IsSent = true;
        SentAt = DateTime.UtcNow;
        ErrorMessage = null;
    }

    /// <summary>
    /// Marks the email as failed with error message.
    /// </summary>
    public void MarkAsFailed(string errorMessage)
    {
        IsSent = false;
        SentAt = null;
        ErrorMessage = errorMessage;
    }
}
