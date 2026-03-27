using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Services.Pdf;

public class ContractPdfService(IApplicationDbContext context, IR2StorageService storageService) : IContractPdfService
{
    public async Task<string> GenerateAndUploadContractPdfAsync(Guid contractId, CancellationToken cancellationToken = default)
    {
        var contract = await context.Contracts
            .Include(c => c.Room)
                .ThenInclude(r => r.Property)
            .Include(c => c.Customer)
            .FirstOrDefaultAsync(c => c.Id == contractId, cancellationToken);

        if (contract == null) throw new Exception("Contract not found");

        // Generate PDF bytes using QuestPDF
        var pdfBytes = GeneratePdfBytes(contract);

        // Upload to Cloudflare R2
        string key = $"contracts/{contractId}/contract.pdf";
        using var stream = new MemoryStream(pdfBytes);
        string pdfUrl = await storageService.UploadAsync(stream, key, "application/pdf", cancellationToken);

        contract.PdfUrl = pdfUrl;
        await context.SaveChangesAsync(cancellationToken);

        return pdfUrl;
    }

    private byte[] GeneratePdfBytes(Contract contract)
    {
        // QuestPDF.Settings.License = LicenseType.Community; // Should be set in Program.cs

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Verdana));

                page.Header().Text("HỢP ĐỒNG THUÊ PHÒNG")
                    .SemiBold().FontSize(20).FontColor(Colors.Blue.Medium).AlignCenter();

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Spacing(10);
                    x.Item().Text($"Mã hợp đồng: {contract.ContractCode}").Bold();
                    x.Item().Text($"Ngày bắt đầu: {contract.StartDate:dd/MM/yyyy}");
                    x.Item().Text($"Ngày kết thúc: {contract.EndDate:dd/MM/yyyy}");
                    
                    x.Item().PaddingTop(10).Text("BÊN CHO THUÊ").Bold();
                    x.Item().Text(contract.Room.Property.Name);
                    x.Item().Text(contract.Room.Property.Address);

                    x.Item().PaddingTop(10).Text("BÊN THUÊ").Bold();
                    x.Item().Text(contract.Customer.FullName);
                    x.Item().Text($"Số CCCD: {contract.Customer.IdCardNumber}");
                    x.Item().Text($"SĐT: {contract.Customer.Phone}");

                    x.Item().PaddingTop(10).Text("CHI TIẾT PHÒNG THUÊ").Bold();
                    x.Item().Text($"Phòng: {contract.Room.RoomNumber} (Tầng {contract.Room.Floor})");
                    x.Item().Text($"Giá thuê: {contract.MonthlyRent:N0} VNĐ/tháng");
                    x.Item().Text($"Tiền cọc: {contract.DepositAmount:N0} VNĐ");

                    x.Item().PaddingTop(20).Text("ĐIỀU KHOẢN").Bold();
                    x.Item().Text(contract.Terms ?? "Theo quy định chung của nhà trọ.");
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Trang ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }
}
