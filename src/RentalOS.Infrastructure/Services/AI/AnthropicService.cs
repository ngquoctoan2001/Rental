#pragma warning disable CS9113 // Parameter is reserved for future use
using System.Runtime.CompilerServices;
using System.Text.Json;
using RentalOS.Application.Common.Interfaces;
using RentalOS.Application.Common.Models;

namespace RentalOS.Infrastructure.Services.AI;

public class AnthropicService(IHttpClientFactory _httpClientFactory) : IAiStreamingService
{
    private const string Model = "claude-sonnet-4-20250514";
    
    public async IAsyncEnumerable<AiStreamChunk> StreamChatAsync(
        IEnumerable<object> messages, 
        string systemPrompt,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        // 1. Build Tools (15 tools total)
        var tools = GetToolDefinitions();

        // 2. Call Anthropic API with streaming: true
        // Mocking behavior for development
        yield return new AiStreamChunk(AiChunkType.Text, "Xin chào! Tôi là trợ lý AI của RentalOS. ");
        yield return new AiStreamChunk(AiChunkType.Done, "");
    }

    private object[] GetToolDefinitions() => new object[]
    {
        new { name = "room_list", description = "Lấy danh sách phòng hiện có" },
        new { name = "room_create", description = "Tạo phòng mới (cần xác nhận)" },
        new { name = "customer_search", description = "Tìm kiếm khách thuê theo tên/SĐT" },
        new { name = "customer_create", description = "Thêm mới khách thuê" },
        new { name = "contract_list", description = "Danh sách hợp đồng hiện tại" },
        new { name = "contract_expiry_list", description = "Các hợp đồng sắp hết hạn" },
        new { name = "contract_create", description = "Tạo hợp đồng thuê mới" },
        new { name = "invoice_list", description = "Danh sách hóa đơn (tất cả hoặc theo phòng)" },
        new { name = "invoice_generate", description = "Chốt điện nước và tạo hóa đơn hàng loạt" },
        new { name = "invoice_send", description = "Gửi thông báo hóa đơn qua Zalo/SMS/Email" },
        new { name = "revenue_report", description = "Báo cáo doanh thu theo tháng/năm" },
        new { name = "room_status_overview", description = "Xem tỷ lệ lấp đầy toàn bộ hệ thống" },
        new { name = "pending_meter_list", description = "Danh sách các phòng chưa chốt điện nước" },
        new { name = "notification_status", description = "Kiểm tra log gửi thông báo" },
        new { name = "meter_reading_save", description = "Ghi chỉ số điện nước cho phòng" }
    };
}

