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
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email cannot be null or empty", nameof(recipientEmail));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name cannot be null or empty", nameof(productName));

        var subject = "⚠️ Alerta de Stock Bajo - SuperPOS";
        var body = BuildLowStockEmailBody(productName, barcode, currentStock);

        return await SendEmailAsync(recipientEmail, subject, body, "StockAlert", cancellationToken);
    }

    public async Task<bool> SendPasswordResetCodeAsync(
        string recipientEmail,
        string userName,
        string code,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email cannot be null or empty", nameof(recipientEmail));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be null or empty", nameof(userName));

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Reset code cannot be null or empty", nameof(code));

        var subject = "🔐 Código de Recuperación de Contraseña - SuperPOS";
        var body = BuildPasswordResetEmailBody(userName, code);

        return await SendEmailAsync(recipientEmail, subject, body, "PasswordReset", cancellationToken);
    }

    public async Task<bool> SendPasswordChangedNotificationAsync(
        string recipientEmail,
        string userName,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(recipientEmail))
            throw new ArgumentException("Recipient email cannot be null or empty", nameof(recipientEmail));

        if (string.IsNullOrWhiteSpace(userName))
            throw new ArgumentException("User name cannot be null or empty", nameof(userName));

        var subject = "✅ Contraseña Cambiada - SuperPOS";
        var body = BuildPasswordChangedEmailBody(userName);

        return await SendEmailAsync(recipientEmail, subject, body, "PasswordChanged", cancellationToken);
    }

    public async Task<bool> SendEmailAsync(
        string to,
        string subject,
        string body,
        string emailType,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(to))
            throw new ArgumentException("Recipient email cannot be null or empty", nameof(to));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or empty", nameof(subject));

        if (string.IsNullOrWhiteSpace(body))
            throw new ArgumentException("Body cannot be null or empty", nameof(body));

        if (string.IsNullOrWhiteSpace(emailType))
            throw new ArgumentException("Email type cannot be null or empty", nameof(emailType));

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

    private string BuildPasswordResetEmailBody(string userName, string code)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
        .code-box {{ background-color: white; border: 2px dashed #4CAF50; padding: 20px; text-align: center; margin: 20px 0; border-radius: 5px; }}
        .code {{ font-size: 36px; font-weight: bold; color: #4CAF50; letter-spacing: 8px; font-family: monospace; }}
        .info {{ background-color: #e3f2fd; border-left: 4px solid #2196F3; padding: 15px; margin: 20px 0; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>🔐 Recuperación de Contraseña</h1>
        </div>
        <div class='content'>
            <p>Hola <strong>{userName}</strong>,</p>
            <p>Hemos recibido una solicitud para restablecer tu contraseña en SuperPOS. Utiliza el siguiente código de verificación:</p>
            <div class='code-box'>
                <div class='code'>{code}</div>
            </div>
            <div class='info'>
                <strong>Información importante:</strong>
                <ul style='margin: 10px 0; padding-left: 20px;'>
                    <li>Este código es válido por <strong>15 minutos</strong></li>
                    <li>Solo puedes intentar validarlo <strong>3 veces</strong></li>
                    <li>Este código es de un solo uso</li>
                </ul>
            </div>
            <div class='warning'>
                <strong>¿No solicitaste este cambio?</strong><br>
                Si no realizaste esta solicitud, puedes ignorar este correo de forma segura. Tu contraseña no será cambiada sin el código de verificación.
            </div>
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

    private string BuildPasswordChangedEmailBody(string userName)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .header {{ background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }}
        .content {{ background-color: #f9f9f9; padding: 20px; border-radius: 0 0 5px 5px; }}
        .success {{ background-color: #d4edda; border-left: 4px solid #28a745; padding: 15px; margin: 20px 0; }}
        .warning {{ background-color: #fff3cd; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; }}
        .footer {{ text-align: center; margin-top: 20px; color: #666; font-size: 12px; }}
        .icon {{ font-size: 48px; text-align: center; margin: 20px 0; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>✅ Contraseña Cambiada</h1>
        </div>
        <div class='content'>
            <div class='icon'>🎉</div>
            <p>Hola <strong>{userName}</strong>,</p>
            <div class='success'>
                <strong>¡Tu contraseña ha sido cambiada exitosamente!</strong><br>
                A partir de ahora, deberás usar tu nueva contraseña para iniciar sesión en SuperPOS.
            </div>
            <p>Por seguridad, todas las sesiones activas en otros dispositivos han sido cerradas automáticamente. Deberás iniciar sesión nuevamente en cada dispositivo.</p>
            <p><strong>Fecha del cambio:</strong> {DateTime.UtcNow:dd/MM/yyyy HH:mm} UTC</p>
            <div class='warning'>
                <strong>¿No realizaste este cambio?</strong><br>
                Si no cambiaste tu contraseña, tu cuenta puede estar comprometida. Contacta inmediatamente al administrador del sistema.
            </div>
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
