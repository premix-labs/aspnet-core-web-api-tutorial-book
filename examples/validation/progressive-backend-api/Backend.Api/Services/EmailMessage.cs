namespace Backend.Api.Services;

public record EmailMessage(
    string To,
    string Subject,
    string Body);
