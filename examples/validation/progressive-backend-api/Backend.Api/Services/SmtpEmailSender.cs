using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Backend.Api.Options;

namespace Backend.Api.Services;

public class SmtpEmailSender(IOptions<EmailOptions> options) : IEmailSender
{
    private readonly EmailOptions optionsValue = options.Value;

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        using var mailMessage = new MailMessage
        {
            From = new MailAddress(optionsValue.FromAddress, optionsValue.FromName),
            Subject = message.Subject,
            Body = message.Body
        };

        mailMessage.To.Add(message.To);

        using var smtpClient = new SmtpClient(optionsValue.SmtpHost, optionsValue.SmtpPort)
        {
            EnableSsl = optionsValue.SmtpEnableSsl
        };

        if (!string.IsNullOrWhiteSpace(optionsValue.SmtpUsername))
        {
            smtpClient.Credentials = new NetworkCredential(
                optionsValue.SmtpUsername,
                optionsValue.SmtpPassword);
        }

        await smtpClient.SendMailAsync(mailMessage, cancellationToken);
    }
}
