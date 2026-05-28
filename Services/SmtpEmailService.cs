using System.Net;
using System.Net.Mail;

namespace InternshipPortal.Services;

public sealed class SmtpEmailService(IConfiguration configuration) : IEmailService
{
    public async Task SendAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var section = configuration.GetSection("Smtp");
        var host = section["Host"] ?? throw new InvalidOperationException("SMTP Host is missing.");
        var port = int.Parse(section["Port"] ?? throw new InvalidOperationException("SMTP Port is missing."));
        var userName = section["UserName"] ?? throw new InvalidOperationException("SMTP UserName is missing.");
        var password = section["Password"] ?? throw new InvalidOperationException("SMTP Password is missing.");
        var fromEmail = section["FromEmail"] ?? userName;
        var fromName = section["FromName"] ?? "Internship Portal";
        var enableSsl = bool.Parse(section["EnableSsl"] ?? "true");

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = subject,
            Body = body,
            IsBodyHtml = false
        };

        message.To.Add(toEmail);

        using var client = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            Credentials = new NetworkCredential(userName, password)
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}