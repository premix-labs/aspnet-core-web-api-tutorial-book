using Backend.Api.Data;
using Backend.Api.Models;

namespace Backend.Api.Services;

public class AuditLogService(AppDbContext db)
{
    public async Task LogAsync(
        Guid? actorUserId,
        string action,
        string entityName,
        string entityId,
        string? ipAddress,
        string? detail)
    {
        db.AuditLogs.Add(new AuditLog
        {
            ActorUserId = actorUserId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            IpAddress = ipAddress,
            Detail = detail
        });

        await db.SaveChangesAsync();
    }
}
