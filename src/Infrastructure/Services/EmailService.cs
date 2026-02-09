using Application.Interfaces.Services;
using Domain.Entities.Emails;
using MailKit.Net.Smtp;
using MimeKit;

namespace Infrastructure.Services;

/// <summary>
/// Email service implementation using MailKit.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IUnitOfWork _unitOfWork;

    public EmailService(IConfiguration configuration, IUnitOfWork unitOfWork)
    {
        _configuration = configuration;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> SendLowStockAlertAsync(
        string recipientEmail,
        string productName,
        string? barcode,
        int currentStock,
        CancellationToken cancellationToken = default)
    {
        var subject = "⚠️ Alerta de Stock Bajo - SuperPOS";
        var body = BuildLowStockEmailBody(productName, barcode, currentStock);

        return await SendEmailAsync(recipientEmail, subject, body, "StockAlert", cancellationToken);
    }

    public async Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        string emailType,
        CancellationToken cancellationToken = default)
    {
        // Create email log entry
        var emailLog = EmailLog.Create(to, subject, body, emailType);

        try
        {
            // Get SMTP configuration
            var smtpServer = _configuration["EmailSettings:SmtpServer"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"] ?? "587");
            var senderEmail = _configuration["EmailSettings:SenderEmail"] ?? "noreply@superpos.com";
            var senderName = _configuration["EmailSettings:SenderName"] ?? "SuperPOS System";
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var enableSsl = bool.Parse(_configuration["EmailSettings:EnableSsl"] ?? "true");

            // Create email message
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            message.Body = builder.ToMessageBody();

            // Send email
            using var client = new SmtpClient();
            await client.ConnectAsync(smtpServer, smtpPort, enableSsl, cancellationToken);

            if (!string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password))
            {
                await client.AuthenticateAsync(username, password, cancellationToken);
            }

            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            // Mark as sent
            emailLog.MarkAsSent();

            // Save email log
            _unitOfWork.Repository<EmailLog>().Add(emailLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            // Mark as failed
            emailLog.MarkAsFailed(ex.Message);

            // Save email log
            _unitOfWork.Repository<EmailLog>().Add(emailLog);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return false;
        }
    }

    private string BuildLowStockEmailBody(string productName, string? barcode, int currentStock)
    {
        var barcodeInfo = string.IsNullOrEmpty(barcode) ? "" : $"<p><strong>Código de Barras:</strong> {barcode}</p>";

        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #ff6b6b; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
        .alert {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .stock-info {{ background-color: white; padding: 15px; border-radius: 5px; margin: 15px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>⚠️ Alerta de Stock Bajo</h1>
        </div>
        <div class='content'>
            <div class='alert'>
                <strong>Atención:</strong> El siguiente producto ha alcanzado un nivel de stock bajo y requiere reabastecimiento.
            </div>
            <div class='stock-info'>
                <h2>{productName}</h2>
                {barcodeInfo}
                <p><strong>Stock Actual:</strong> <span style='color: #ff6b6b; font-size: 24px; font-weight: bold;'>{currentStock} unidades</span></p>
                <p><strong>Umbral de Alerta:</strong> 10 unidades</p>
            </div>
            <p>Por favor, considere realizar un pedido de reabastecimiento lo antes posible para evitar quiebre de stock.</p>
            <p>Este es un mensaje automático del sistema SuperPOS.</p>
        </div>
        <div class='footer'>
            <p>SuperPOS - Sistema de Punto de Venta</p>
            <p>Este es un correo automático, por favor no responder.</p>
        </div>
    </div>
</body>
</html>
";
    }
}
