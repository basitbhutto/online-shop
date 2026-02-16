using Application.Interfaces;
using Microsoft.Extensions.Options;
using Shared.Settings;
using System.Net;
using System.Net.Mail;

namespace Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        using var client = new SmtpClient(_settings.SmtpServer, _settings.SmtpPort)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.FromEmail, _settings.AppPassword)
        };

        var mail = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        mail.To.Add(to);

        await client.SendMailAsync(mail, cancellationToken);
    }

    public Task SendOtpAsync(string to, string otp, CancellationToken cancellationToken = default)
    {
        var html = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family:Segoe UI,Arial,sans-serif;background:#f5f5f5;padding:20px'>
<div style='max-width:400px;margin:0 auto;background:#fff;border-radius:8px;padding:30px;box-shadow:0 2px 10px rgba(0,0,0,.1)'>
<h2 style='color:#dc3545;margin-top:0'>Shopwala</h2>
<p>Your verification code is:</p>
<p style='font-size:28px;font-weight:bold;letter-spacing:8px;color:#212529'>{otp}</p>
<p style='color:#6c757d;font-size:14px'>This code expires in 10 minutes. Do not share it with anyone.</p>
<hr style='border:none;border-top:1px solid #eee'/>
<p style='color:#6c757d;font-size:12px'>Karachi's Trusted Online Store</p>
</div>
</body>
</html>";
        return SendEmailAsync(to, "Verify your email - Shopwala", html, cancellationToken);
    }

    public Task SendOrderConfirmationAsync(string to, string customerName, int orderId, decimal total, CancellationToken cancellationToken = default)
    {
        var html = $@"
<!DOCTYPE html>
<html>
<head><meta charset='utf-8'></head>
<body style='font-family:Segoe UI,Arial,sans-serif;background:#f5f5f5;padding:20px'>
<div style='max-width:500px;margin:0 auto;background:#fff;border-radius:8px;padding:30px;box-shadow:0 2px 10px rgba(0,0,0,.1)'>
<h2 style='color:#dc3545;margin-top:0'>Thank you for your order!</h2>
<p>Hi {customerName},</p>
<p>Your order <strong>#{orderId}</strong> has been placed successfully.</p>
<p><strong>Total: Rs {total:N0}</strong></p>
<p>We'll deliver to your address in Karachi. You can track your order in your account.</p>
<hr style='border:none;border-top:1px solid #eee'/>
<p style='color:#6c757d;font-size:12px'>Shopwala - Karachi's Trusted Online Store</p>
</div>
</body>
</html>";
        return SendEmailAsync(to, $"Order #{orderId} Confirmed - Shopwala", html, cancellationToken);
    }
}
