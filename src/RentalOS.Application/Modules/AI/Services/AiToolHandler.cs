#pragma warning disable CS9113 // Parameter is reserved for future use
using MediatR;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Modules.Customers.Queries.SearchCustomers;
using RentalOS.Application.Modules.Properties.Queries.GetPropertyStats;
using RentalOS.Application.Modules.Reports.Queries.GetRevenueReport;
using RentalOS.Application.Modules.Rooms.Queries.GetRooms;
using System.Text.Json;

namespace RentalOS.Application.Modules.AI.Services;

public class AiToolHandler(ISender mediator, ITenantContext _tenantContext)
{
    public async Task<string> ExecuteToolAsync(string toolName, string toolInputJson)
    {
        // 1. Parse input
        using var doc = JsonDocument.Parse(toolInputJson);
        var root = doc.RootElement;

        // 2. Map to MediatR
        object? result = toolName switch
        {
            "room_list" => await mediator.Send(new GetRoomsQuery()),
            "room_create" => "Vui lòng xác nh?n t?o phòng m?i v?i thông tin: " + toolInputJson, // Require confirmation
            "customer_search" => await mediator.Send(new SearchCustomersQuery(root.GetProperty("query").GetString()!)),
            "revenue_report" => await mediator.Send(new GetRevenueReportQuery(root.GetProperty("month").GetString() ?? "this_month")),
            "room_status_overview" => await mediator.Send(new GetPropertyStatsQuery(Guid.Empty)),
            // ... các tool còn l?i mapping tuong t?
            _ => "Tool không h? tr? ho?c dang phát tri?n."
        };

        return JsonSerializer.Serialize(result);
    }
}
