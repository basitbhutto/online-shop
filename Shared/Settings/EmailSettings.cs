namespace Shared.Settings;

public class EmailSettings
{
    public const string SectionName = "EmailSettings";
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = "Shopwala";
    public string AppPassword { get; set; } = string.Empty;
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
}
