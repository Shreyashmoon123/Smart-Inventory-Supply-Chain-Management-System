
using Microsoft.EntityFrameworkCore;
using SmartInventory.Application.DTOs.AuditLogs;
using SmartInventory.Application.Interfaces;
using SmartInventory.Core.Entities;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class AuditLogService : IAuditLogService
{
    private readonly AppDbContext _context;

    public AuditLogService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuditLogResponseDto>> GetAllAsync(
        int? userId,
        string? action,
        string? entityName,
        DateTime? fromDate,
        DateTime? toDate)
    {
        var query = _context.AuditLogs
            .AsNoTracking()
            .Include(a => a.User)
            .AsQueryable();

        if (userId.HasValue)
            query = query.Where(a => a.UserId == userId.Value);

        if (!string.IsNullOrWhiteSpace(action))
            query = query.Where(a => a.Action.Contains(action));

        if (!string.IsNullOrWhiteSpace(entityName))
            query = query.Where(a => a.EntityName == entityName);

        if (fromDate.HasValue)
            query = query.Where(a => a.CreatedAt >= fromDate.Value);

        if (toDate.HasValue)
            query = query.Where(a => a.CreatedAt < toDate.Value.Date.AddDays(1));

        return await query
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new AuditLogResponseDto
            {
                AuditLogId = a.AuditLogId,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.FullName : null,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Details = a.Details,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<AuditLogResponseDto?> GetByIdAsync(int id)
    {
        return await _context.AuditLogs
            .AsNoTracking()
            .Where(a => a.AuditLogId == id)
            .Select(a => new AuditLogResponseDto
            {
                AuditLogId = a.AuditLogId,
                UserId = a.UserId,
                UserName = a.User != null ? a.User.FullName : null,
                Action = a.Action,
                EntityName = a.EntityName,
                EntityId = a.EntityId,
                Details = a.Details,
                CreatedAt = a.CreatedAt
            })
            .FirstOrDefaultAsync();
    }

    public async Task<AuditLogResponseDto> CreateAsync(
        CreateAuditLogDto dto,
        int? userId)
    {
        if (userId.HasValue &&
            !await _context.Users.AnyAsync(u => u.UserId == userId.Value))
        {
            throw new InvalidOperationException("User not found.");
        }

        var auditLog = new AuditLog
        {
            UserId = userId,
            Action = dto.Action.Trim(),
            EntityName = dto.EntityName?.Trim(),
            EntityId = dto.EntityId?.Trim(),
            Details = dto.Details?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(auditLog.AuditLogId)
            ?? throw new InvalidOperationException("Audit log could not be retrieved.");
    }
}
