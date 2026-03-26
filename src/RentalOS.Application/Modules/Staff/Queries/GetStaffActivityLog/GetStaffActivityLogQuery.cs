using Microsoft.EntityFrameworkCore;

using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Staff.Queries.GetStaffActivityLog;

public record GetStaffActivityLogQuery(Guid StaffId, int Page = 1, int PageSize = 20) : IRequest<List<ActivityLogDto>>;

public class ActivityLogDto
{
    public Guid Id { get; set; }
    public string Action { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class GetStaffActivityLogQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetStaffActivityLogQuery, List<ActivityLogDto>>
{
    public async Task<List<ActivityLogDto>> Handle(GetStaffActivityLogQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        const string sql = @"
            SELECT id, action, entity_type as EntityType, details, created_at as CreatedAt
            FROM audit_logs
            WHERE user_id = @userId
            ORDER BY created_at DESC
            LIMIT @pageSize OFFSET @offset";

        var logs = await connection.QueryAsync<ActivityLogDto>(sql, new
        {
            userId = request.StaffId.ToString(),
            pageSize = request.PageSize,
            offset = (request.Page - 1) * request.PageSize
        });

        return logs.ToList();
    }
}
