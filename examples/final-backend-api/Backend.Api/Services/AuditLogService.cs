using Backend.Api.Data;
using Backend.Api.Models;

namespace Backend.Api.Services;

public class AuditLogService(AppDbContext dbContext) : IAuditLogService
{
    public async Task WriteAsync(
        Guid? actorUserId,
        string action,
        string entityName,
        string entityId,
        string? ipAddress,
        string? detail,
        CancellationToken cancellationToken)
    {
        dbContext.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            IpAddress = ipAddress,
            Detail = detail
        });

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
