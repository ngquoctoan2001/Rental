using ClosedXML.Excel;
using MediatR;
using Microsoft.EntityFrameworkCore;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Application.Modules.Transactions.Queries.ExportTransactions;

public record ExportTransactionsQuery : IRequest<Result<byte[]>>
{
    public DateTime? DateFrom { get; init; }
    public DateTime? DateTo { get; init; }
}

public class ExportTransactionsQueryHandler(IApplicationDbContext context) : IRequestHandler<ExportTransactionsQuery, Result<byte[]>>
{
    public async Task<Result<byte[]>> Handle(ExportTransactionsQuery request, CancellationToken cancellationToken)
    {
        var query = context.Transactions
            .Include(t => t.Invoice)
                .ThenInclude(i => i!.Contract)
                    .ThenInclude(c => c.Customer)
            .Include(t => t.Invoice)
                .ThenInclude(i => i!.Contract)
                    .ThenInclude(c => c.Room)
            .AsNoTracking();

        if (request.DateFrom.HasValue)
            query = query.Where(t => t.PaidAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
            query = query.Where(t => t.PaidAt <= request.DateTo.Value);

        var transactions = await query.OrderByDescending(t => t.PaidAt).ToListAsync(cancellationToken);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Transactions");

        // Headers
        worksheet.Cell(1, 1).Value = "Ngày thanh toán";
        worksheet.Cell(1, 2).Value = "Mã hóa đơn";
        worksheet.Cell(1, 3).Value = "Khách hàng";
        worksheet.Cell(1, 4).Value = "Phòng";
        worksheet.Cell(1, 5).Value = "Số tiền";
        worksheet.Cell(1, 6).Value = "Phương thức";
        worksheet.Cell(1, 7).Value = "Loại";
        worksheet.Cell(1, 8).Value = "Ghi chú";

        // Style headers
        var headerRange = worksheet.Range(1, 1, 1, 8);
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Fill.BackgroundColor = XLColor.LightGray;

        // Data
        for (int i = 0; i < transactions.Count; i++)
        {
            var t = transactions[i];
            int row = i + 2;
            worksheet.Cell(row, 1).Value = t.PaidAt.ToString("dd/MM/yyyy HH:mm");
            worksheet.Cell(row, 2).Value = t.Invoice?.InvoiceCode ?? "-";
            worksheet.Cell(row, 3).Value = t.Invoice?.Contract.Customer.FullName ?? "-";
            worksheet.Cell(row, 4).Value = t.Invoice?.Contract.Room.RoomNumber ?? "-";
            worksheet.Cell(row, 5).Value = (double)t.Amount;
            worksheet.Cell(row, 6).Value = t.Method.ToString();
            worksheet.Cell(row, 7).Value = t.Category.ToString();
            worksheet.Cell(row, 8).Value = t.Note;
        }

        worksheet.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        workbook.SaveAs(ms);
        return Result<byte[]>.Ok(ms.ToArray());
    }
}
