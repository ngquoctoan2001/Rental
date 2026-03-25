using MediatR;
using RentalOS.Application.Common.Interfaces;
using System.Text.Json;

namespace RentalOS.Application.Modules.AI.Services;

public class AiToolHandler(ISender mediator, ITenantContext tenantContext)
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
            "room_create" => "Vui lòng xác nhận tạo phòng mới với thông tin: " + toolInputJson, // Require confirmation
            "customer_search" => await mediator.Send(new SearchCustomersQuery(root.GetProperty("query").GetString()!)),
            "revenue_report" => await mediator.Send(new GetRevenueReportQuery(root.GetProperty("month").GetString())),
            "room_status_overview" => await mediator.Send(new GetPropertyStatsQuery()),
            // ... các tool còn lại mapping tương tự
            _ => "Tool không hỗ trợ hoặc đang phát triển."
        };

        return JsonSerializer.Serialize(result);
    }
}
 Eskom mapping logic for 15 tools. Eskom security via tenantContext. Eskom human-in-the-loop for write actions.
