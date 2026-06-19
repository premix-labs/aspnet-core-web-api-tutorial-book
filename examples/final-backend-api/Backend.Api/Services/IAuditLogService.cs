namespace Backend.Api.Services;

public interface IAuditLogService
{
    Task WriteAsync(
        Guid? actorUserId,
        string action,
        string entityName,
        string entityId,
        string? ipAddress,
        string? detail,
        CancellationToken cancellationToken);
}
