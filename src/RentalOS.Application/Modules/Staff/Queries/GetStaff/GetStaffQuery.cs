using Microsoft.EntityFrameworkCore;

using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Staff.Queries.GetStaff;

public record GetStaffQuery : IRequest<List<StaffListItemDto>>;

public class StaffListItemDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
}

public class GetStaffQueryHandler(IApplicationDbContext dbContext, ITenantContext tenantContext)
    : IRequestHandler<GetStaffQuery, List<StaffListItemDto>>
{
    public async Task<List<StaffListItemDto>> Handle(GetStaffQuery request, CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        const string sql = @"
            SELECT id, email, full_name as FullName, role, is_active as IsActive, created_at as CreatedAt, last_login_at as LastLoginAt
            FROM users
            WHERE tenant_id = @tenantId AND role != 'owner'
            ORDER BY created_at DESC";

        var staff = await connection.QueryAsync<StaffListItemDto>(sql, new { tenantId = tenantContext.TenantId });
        return staff.ToList();
    }
}
