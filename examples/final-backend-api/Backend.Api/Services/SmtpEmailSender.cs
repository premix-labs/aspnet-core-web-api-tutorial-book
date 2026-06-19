using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Backend.Api.Options;

namespace Backend.Api.Services;

public class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(_options.FromAddress, _options.FromName),
            Subject = message.Subject,
            Body = message.Body
        };

        mailMessage.To.Add(message.To);

        using var smtpClient = new SmtpClient(_options.SmtpHost, _options.SmtpPort)
        {
            EnableSsl = _options.SmtpEnableSsl
        };

        if (!string.IsNullOrWhiteSpace(_options.SmtpUsername))
        {
            smtpClient.Credentials = new NetworkCredential(
                _options.SmtpUsername,
                _options.SmtpPassword);
        }

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }
}
