namespace Application.Interfaces;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    Task SendOtpAsync(string to, string otp, CancellationToken cancellationToken = default);
    Task SendOrderConfirmationAsync(string to, string customerName, long orderId, decimal total, CancellationToken cancellationToken = default);
}
