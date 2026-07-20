using BankApp.BankApp.Common.Interfaces.Services;
using BankApp.BankApp.Common.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace BankApp.BankApp.Services;

public class EmailService : IEmailService
{
    private readonly SmtpSettings _settings;

    public EmailService(IOptions<SmtpSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string resetLink, CancellationToken cancellationToken = default)
    {
        using var message = new MimeMessage();

        message.From.Add(new MailboxAddress(_settings.FromName, _settings.FromAddress));
        message.To.Add(new MailboxAddress(string.Empty, toEmail));
        message.Subject = "Password Reset - BankApp";

        message.Body = new TextPart(TextFormat.Html)
        {
            Text = $"""
                <h2>Password Reset</h2>
                <p>You requested a password reset. Click the link below to set a new password:</p>
                <p><a href="{resetLink}">Reset Password</a></p>
                <p>This link expires in 15 minutes.</p>
                <p>If you did not request this, you can ignore this email.</p>
                """
        };

        using var client = new SmtpClient();
        await client.ConnectAsync(_settings.Host, _settings.Port, SecureSocketOptions.StartTls, cancellationToken);
        await client.AuthenticateAsync(_settings.Username, _settings.Password, cancellationToken);
        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
