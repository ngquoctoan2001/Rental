using MediatR;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Services;
using Microsoft.EntityFrameworkCore;
using RentalOS.Domain.Enums;

namespace RentalOS.Application.Common.Behaviors;

public class PlanEnforcementBehavior<TRequest, TResponse>(
    ITenantContext tenantContext,
    IApplicationDbContext dbContext,
    IPlanService planService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        if (tenantId == Guid.Empty) return await next();

        var tenant = await dbContext.Tenants.FindAsync(new object[] { tenantId }, cancellationToken);
        if (tenant == null) return await next();

        // 1. Check Expiration
        if (planService.IsTrialExpired(tenant) || planService.IsPlanExpired(tenant))
        {
            // Allowed commands even if expired (e.g. Upgrade, Cancel, Logout)
            var typeName = typeof(TRequest).Name;
            if (!typeName.Contains("Upgrade") && !typeName.Contains("Subscription") && !typeName.Contains("Auth"))
                throw new Exception("Gói cước đã hết hạn. Vui lòng nâng cấp để tiếp tục sử dụng.");
        }

        // 2. Resource Limits
        var commandName = typeof(TRequest).Name;

        if (commandName.Contains("CreateRoom"))
        {
            var count = await dbContext.Rooms.CountAsync(cancellationToken);
            if (count >= planService.GetRoomLimit(tenant.Plan))
                throw new Exception($"Gói {tenant.Plan} giới hạn {planService.GetRoomLimit(tenant.Plan)} phòng. Vui lòng nâng cấp.");
        }

        if (commandName.Contains("CreateProperty"))
        {
            var count = await dbContext.Properties.CountAsync(cancellationToken);
            if (count >= planService.GetPropertyLimit(tenant.Plan))
                throw new Exception($"Gói {tenant.Plan} giới hạn {planService.GetPropertyLimit(tenant.Plan)} nhà trọ. Vui lòng nâng cấp.");
        }

        // 3. Feature Access
        if (commandName.Contains("Ai") && !planService.HasFeature(tenant.Plan, "ai"))
            throw new Exception("Tính năng AI yêu cầu gói Pro trở lên.");

        if (commandName.Contains("Report") && !planService.HasFeature(tenant.Plan, "reports"))
            throw new Exception("Tính năng Báo cáo nâng cao yêu cầu gói Pro trở lên.");

        return await next();
    }
}
