
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartInventory.Core.Entities;
using Microsoft.AspNetCore.Http;
namespace SmartInventory.Infrastructure.Interceptors;

public class AuditLogInterceptor : SaveChangesInterceptor
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditLogInterceptor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        AddAuditLogs(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void AddAuditLogs(DbContext? context)
    {
        if (context == null)
            return;

        var userIdClaim = _httpContextAccessor.HttpContext?
            .User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        int? userId = int.TryParse(userIdClaim, out var parsedUserId)
            ? parsedUserId
            : null;

        var entries = context.ChangeTracker
            .Entries()
            .Where(e =>
                e.Entity is not AuditLog &&
                (e.State == EntityState.Added ||
                 e.State == EntityState.Modified ||
                 e.State == EntityState.Deleted))
            .ToList();

        var auditLogs = new List<AuditLog>();

        foreach (var entry in entries)
        {
            var action = entry.State switch
            {
                EntityState.Added => "Created",
                EntityState.Modified => "Updated",
                EntityState.Deleted => "Deleted",
                _ => string.Empty
            };

            var entityName = entry.Metadata.ClrType.Name;

            var keyProperties = entry.Properties
                .Where(p => p.Metadata.IsPrimaryKey())
                .ToList();

            string? entityId = null;

            if (keyProperties.Count > 0 &&
                keyProperties.All(p => !p.IsTemporary && p.CurrentValue != null))
            {
                entityId = string.Join(
                    ",",
                    keyProperties.Select(p => p.CurrentValue?.ToString()));
            }

            var changes = new Dictionary<string, object?>();

            foreach (var property in entry.Properties)
            {
                var propertyName = property.Metadata.Name;

                if (propertyName.Equals(
                    "PasswordHash",
                    StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                if (property.Metadata.IsPrimaryKey())
                    continue;

                if (entry.State == EntityState.Added)
                {
                    changes[propertyName] = new
                    {
                        NewValue = property.CurrentValue
                    };
                }
                else if (entry.State == EntityState.Modified &&
                         property.IsModified)
                {
                    changes[propertyName] = new
                    {
                        OldValue = property.OriginalValue,
                        NewValue = property.CurrentValue
                    };
                }
                else if (entry.State == EntityState.Deleted)
                {
                    changes[propertyName] = new
                    {
                        OldValue = property.OriginalValue
                    };
                }
            }

            auditLogs.Add(new AuditLog
            {
                UserId = userId,
                Action = $"{entityName} {action}",
                EntityName = entityName,
                EntityId = entityId,
                Details = changes.Count > 0
                    ? JsonSerializer.Serialize(changes)
                    : null,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (auditLogs.Count > 0)
            context.Set<AuditLog>().AddRange(auditLogs);
    }
}
