using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Domain.Entities;

namespace RentalOS.Infrastructure.Services.Pdf;

public class InvoicePdfService(IApplicationDbContext context, IR2StorageService storageService) : IInvoicePdfService
{
    public async Task<string> GenerateAndUploadInvoicePdfAsync(Guid invoiceId, CancellationToken cancellationToken = default)
    {
        var invoice = await context.Invoices
            .Include(i => i.Contract)
                .ThenInclude(c => c.Room)
                    .ThenInclude(r => r.Property)
            .Include(i => i.Contract)
                .ThenInclude(c => c.Customer)
            .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);

        if (invoice == null) throw new Exception("Invoice not found");

        var pdfBytes = GeneratePdfBytes(invoice);

        // Upload to Cloudflare R2
        string key = $"invoices/{invoiceId}/invoice.pdf";
        using var stream = new MemoryStream(pdfBytes);
        string pdfUrl = await storageService.UploadAsync(stream, key, "application/pdf", cancellationToken);

        invoice.PdfUrl = pdfUrl;
        await context.SaveChangesAsync(cancellationToken);

        return pdfUrl;
    }

    private byte[] GeneratePdfBytes(Invoice invoice)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Verdana));

                page.Header().Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("RentalOS").SemiBold().FontSize(20).FontColor(Colors.Blue.Medium);
                        col.Item().Text($"{invoice.Contract.Room.Property.Name}");
                        col.Item().Text($"{invoice.Contract.Room.Property.Address}");
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("HÓA ĐƠN TIỀN PHÒNG").Bold().FontSize(16).AlignCenter();
                        col.Item().Text($"Số: {invoice.InvoiceCode}").AlignCenter();
                        col.Item().Text($"Tháng: {invoice.BillingMonth:MM/yyyy}").AlignCenter();
                    });
                });

                page.Content().PaddingVertical(1, Unit.Centimetre).Column(x =>
                {
                    x.Spacing(10);
                    
                    x.Item().Row(row => {
                        row.RelativeItem().Text($"Khách thuê: {invoice.Contract.Customer.FullName}");
                        row.RelativeItem().Text($"Phòng: {invoice.Contract.Room.RoomNumber}");
                    });

                    x.Item().LineHorizontal(0.5f);

                    // Bảng chi tiết
                    x.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3);
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                            columns.RelativeColumn();
                        });

                        table.Header(header =>
                        {
                            header.Cell().PaddingBottom(5).Text("Khoản mục").Bold();
                            header.Cell().PaddingBottom(5).Text("Chỉ số").AlignCenter().Bold();
                            header.Cell().PaddingBottom(5).Text("Đơn giá").AlignRight().Bold();
                            header.Cell().PaddingBottom(5).Text("Thành tiền").AlignRight().Bold();
                        });

                        // Tiền phòng
                        table.Cell().Text("Tiền thuê phòng");
                        table.Cell().Text("");
                        table.Cell().AlignRight().Text($"{invoice.RoomRent:N0}");
                        table.Cell().AlignRight().Text($"{invoice.RoomRent:N0}");

                        // Điện
                        table.Cell().Text("Tiền điện");
                        table.Cell().AlignCenter().Text($"{invoice.ElectricityNew - invoice.ElectricityOld} kWh");
                        table.Cell().AlignRight().Text($"{invoice.ElectricityPrice:N0}");
                        table.Cell().AlignRight().Text($"{invoice.ElectricityAmount:N0}");

                        // Nước
                        table.Cell().Text("Tiền nước");
                        table.Cell().AlignCenter().Text($"{invoice.WaterNew - invoice.WaterOld} m³");
                        table.Cell().AlignRight().Text($"{invoice.WaterPrice:N0}");
                        table.Cell().AlignRight().Text($"{invoice.WaterAmount:N0}");

                        // Các phí khác
                        if (invoice.ServiceFee > 0)
                        {
                            table.Cell().Text("Phí dịch vụ");
                            table.Cell().Text("");
                            table.Cell().AlignRight().Text($"{invoice.ServiceFee:N0}");
                            table.Cell().AlignRight().Text($"{invoice.ServiceFee:N0}");
                        }
                        if (invoice.InternetFee > 0)
                        {
                            table.Cell().Text("Tiền Internet");
                            table.Cell().Text("");
                            table.Cell().AlignRight().Text($"{invoice.InternetFee:N0}");
                            table.Cell().AlignRight().Text($"{invoice.InternetFee:N0}");
                        }
                        if (invoice.GarbageFee > 0)
                        {
                            table.Cell().Text("Tiền rác");
                            table.Cell().Text("");
                            table.Cell().AlignRight().Text($"{invoice.GarbageFee:N0}");
                            table.Cell().AlignRight().Text($"{invoice.GarbageFee:N0}");
                        }
                        if (invoice.OtherFees != 0)
                        {
                            table.Cell().Text($"Phí khác ({invoice.OtherFeesNote})");
                            table.Cell().Text("");
                            table.Cell().AlignRight().Text($"{invoice.OtherFees:N0}");
                            table.Cell().AlignRight().Text($"{invoice.OtherFees:N0}");
                        }
                        if (invoice.Discount > 0)
                        {
                            table.Cell().Text($"Giảm trừ ({invoice.DiscountNote})");
                            table.Cell().Text("");
                            table.Cell().AlignRight().Text("");
                            table.Cell().AlignRight().Text($"-{invoice.Discount:N0}");
                        }
                    });

                    x.Item().LineHorizontal(1);
                    x.Item().AlignRight().Text($"TỔNG CỘNG: {invoice.TotalAmount:N0} VNĐ").Bold().FontSize(14);
                    
                    x.Item().PaddingTop(20).Text("Ghi chú:").Italic();
                    x.Item().Text(invoice.Notes ?? "Vui lòng thanh toán trước hạn.");
                    
                    x.Item().PaddingTop(10).Text("Link thanh toán trực tuyến:").Bold();
                    x.Item().Text($"https://rentalos.vn/pay/{invoice.PaymentLinkToken}").FontColor(Colors.Blue.Medium);
                });

                page.Footer().AlignCenter().Text(x =>
                {
                    x.Span("Hóa đơn được tạo tự động bởi RentalOS - ");
                    x.CurrentPageNumber();
                });
            });
        });

        return document.GeneratePdf();
    }
}
