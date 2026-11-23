
using SmartInventory.Application.DTOs.AuditLogs;

namespace SmartInventory.Application.Interfaces;

public interface IAuditLogService
{
    Task<List<AuditLogResponseDto>> GetAllAsync(
        int? userId,
        string? action,
        string? entityName,
        DateTime? fromDate,
        DateTime? toDate);

    Task<AuditLogResponseDto?> GetByIdAsync(int id);

    Task<AuditLogResponseDto> CreateAsync(
        CreateAuditLogDto dto,
        int? userId);
}
