using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace SmartSure.IdentityService.Services;

/// <summary>
/// Sends transactional emails (OTPs, password resets) via SMTP using MailKit.
/// SMTP credentials and host settings are read from configuration / .env.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Composes and sends an HTML email to <paramref name="toEmail"/>.
    /// Connects to the configured SMTP host, authenticates if credentials are present,
    /// sends the message, and disconnects cleanly.
    /// </summary>
    public async Task SendAsync(string toEmail, string subject, string body)
    {
        var message = new MimeMessage();
        var fromEmail = _configuration["Email:FromEmail"] ?? "noreply@smartsure.local";
        var fromName = _configuration["Email:FromName"] ?? "SmartSure";
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };

        using var client = new SmtpClient();
        var host = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var port = int.TryParse(_configuration["Email:SmtpPort"], out var smtpPort) ? smtpPort : 587;

        await client.ConnectAsync(host, port, false);

        // Only authenticate when credentials are configured (allows local dev without SMTP auth)
        var username = _configuration["Email:SmtpUser"];
        var password = _configuration["Email:SmtpPass"];
        if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
        {
            await client.AuthenticateAsync(username, password);
        }

        await client.SendAsync(message);
        await client.DisconnectAsync(true);
    }
}
