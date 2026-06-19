using Backend.Api.Services;

namespace Backend.Api.Tests;

public class TestEmailSender : IEmailSender
{
    private readonly List<EmailMessage> messages = [];
    private readonly object lockObject = new();

    public IReadOnlyCollection<EmailMessage> Messages
    {
        get
        {
            lock (lockObject)
            {
                return messages.ToArray();
            }
        }
    }

    public Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        lock (lockObject)
        {
            messages.Add(message);
        }

        return Task.CompletedTask;
    }
}
