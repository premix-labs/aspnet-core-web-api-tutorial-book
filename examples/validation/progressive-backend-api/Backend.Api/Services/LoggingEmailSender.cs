namespace Backend.Api.Services;

public class LoggingEmailSender(ILogger<LoggingEmailSender> logger) : IEmailSender
{
    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        logger.LogInformation(
            "Email queued for {To}. Subject: {Subject}. Body: {Body}",
            message.To,
            message.Subject,
            message.Body);

        return Task.CompletedTask;
    }
}
