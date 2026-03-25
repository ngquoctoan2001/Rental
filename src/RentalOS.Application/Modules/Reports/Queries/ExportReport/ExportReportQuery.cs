using System.Data;
using ClosedXML.Excel;
using Dapper;
using MediatR;
using RentalOS.Application.Common.Interfaces;

namespace RentalOS.Application.Modules.Reports.Queries.ExportReport;

public record ExportReportQuery(string Type, string Period = "this_month") : IRequest<ExportReportResult>;

public class ExportReportResult
{
    public byte[] Content { get; set; } = [];
    public string FileName { get; set; } = string.Empty;
    public string ContentType => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}

public class ExportReportQueryHandler(IDbConnection dbConnection, ITenantContext tenantContext)
    : IRequestHandler<ExportReportQuery, ExportReportResult>
{
    public async Task<ExportReportResult> Handle(ExportReportQuery request, CancellationToken cancellationToken)
    {
        var tenantId = tenantContext.TenantId;
        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Report");

        if (request.Type.ToLower() == "revenue")
        {
            await FillRevenueSheet(worksheet, tenantId);
        }
        else if (request.Type.ToLower() == "occupancy")
        {
            await FillOccupancySheet(worksheet, tenantId);
        }
        else
        {
            // Default: Transactions or generic
            await FillTransactionsSheet(worksheet, tenantId);
        }

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        
        return new ExportReportResult
        {
            Content = stream.ToArray(),
            FileName = $"Report_{request.Type}_{DateTime.Now:yyyyMMddHHmm}.xlsx"
        };
    }

    private async Task FillRevenueSheet(IXLWorksheet ws, Guid tenantId)
    {
        ws.Cell(1, 1).Value = "Tháng";
        ws.Cell(1, 2).Value = "Doanh thu (VNĐ)";
        
        const string sql = @"
            SELECT TO_CHAR(paid_at, 'YYYY-MM') as Month, SUM(amount) as Amount
            FROM transactions 
            WHERE tenant_id = @tenantId AND direction = 'income' AND is_deleted = false
            GROUP BY Month ORDER BY Month DESC";
        
        var data = await dbConnection.QueryAsync<dynamic>(sql, new { tenantId });
        int row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.month;
            ws.Cell(row, 2).Value = item.amount;
            row++;
        }
    }

    private async Task FillOccupancySheet(IXLWorksheet ws, Guid tenantId)
    {
        ws.Cell(1, 1).Value = "Nhà trọ";
        ws.Cell(1, 2).Value = "Số phòng trống";
        ws.Cell(1, 3).Value = "Số phòng đã thuê";
        ws.Cell(1, 4).Value = "Tổng số phòng";

        const string sql = @"
            SELECT p.name, 
                   COUNT(*) FILTER (WHERE r.status = 'available') as Available,
                   COUNT(*) FILTER (WHERE r.status = 'rented') as Rented,
                   COUNT(*) as Total
            FROM properties p
            JOIN rooms r ON p.id = r.property_id
            WHERE p.tenant_id = @tenantId AND p.is_deleted = false AND r.is_deleted = false
            GROUP BY p.name";

        var data = await dbConnection.QueryAsync<dynamic>(sql, new { tenantId });
        int row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.name;
            ws.Cell(row, 2).Value = item.available;
            ws.Cell(row, 3).Value = item.rented;
            ws.Cell(row, 4).Value = item.total;
            row++;
        }
    }

    private async Task FillTransactionsSheet(IXLWorksheet ws, Guid tenantId)
    {
        ws.Cell(1, 1).Value = "Ngày";
        ws.Cell(1, 2).Value = "Loại";
        ws.Cell(1, 3).Value = "Số tiền";
        ws.Cell(1, 4).Value = "Phương thức";
        ws.Cell(1, 5).Value = "Ghi chú";

        const string sql = @"
            SELECT paid_at, direction, amount, method, note
            FROM transactions
            WHERE tenant_id = @tenantId AND is_deleted = false
            ORDER BY paid_at DESC LIMIT 1000";

        var data = await dbConnection.QueryAsync<dynamic>(sql, new { tenantId });
        int row = 2;
        foreach (var item in data)
        {
            ws.Cell(row, 1).Value = item.paid_at;
            ws.Cell(row, 2).Value = item.direction;
            ws.Cell(row, 3).Value = item.amount;
            ws.Cell(row, 4).Value = item.method;
            ws.Cell(row, 5).Value = item.note;
            row++;
        }
    }
}
